
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using VRC.SDK3.Data;


namespace OpenFlightVRC
{
#if !COMPILER_UDONSHARP && UNITY_EDITOR // These using statements must be wrapped in this check to prevent issues on builds
    using UnityEditor;
    using UdonSharpEditor;
#endif

    // This is a custom inspector for the WingFlightPlusGlide script. It currently just adds a reset to defaults button

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(DesktopFlightPlusGlide))]
    public class DesktopFlightPlusGlideEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DesktopFlightPlusGlide script = (DesktopFlightPlusGlide)target;

            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            if (GUILayout.Button("Reset to Prefab Defaults"))
            {
                // Reset all values to the default in the prefab
                PrefabUtility.RevertObjectOverride(script, InteractionMode.AutomatedAction);
            }

            DrawDefaultInspector();
        }
    }
#endif

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DesktopFlightPlusGlide : LoggableUdonSharpBehaviour
    {
        #region Settings
        /// <summary>
        /// Udon behavior holding the flight properties shared between VR and Desktop flight.
        /// </summary>
        [Tooltip("Reference to udon behavior holding the shared properties between VR and desktop flight.")]
        public FlightProperties FP;

        [Tooltip("The flap strength of a desktop flight")]
        public float DesktopFlapStrengthMod = 0.01f;
		#endregion

		// State Control Variables
		/// <summary>
		/// Cached reference to the local player. This is set in Start() and should not be modified.
		/// </summary>
		private VRCPlayerApi LocalPlayer;

		/// <summary>
		/// The ticks per second in deltatime form.
		/// For example, a value of 0.02f would be 50 ticks per second, or 1/50.
		/// </summary>
		private const float DeltaTimeTicksPerSecond = 1f / 20f;

		/// <summary>
		/// The current time tick value.
		/// It cycles from 0 to 99 at a rate of 50 ticks per second.
		/// </summary>
		private int timeTick = -1; // -1 until the player is valid, then this value cycles from 0-99 at 50 ticks per second


        /// <summary>
		/// Determines whether the controllers are held outside of an imaginary cylinder.
		/// </summary>
		private bool handsOut = false; // Are the controllers held outside of an imaginary cylinder?

		/// <summary>
		/// Indicates whether the hands are in opposite positions.
		/// </summary>
		private bool handsOpposite = false;

		/// <summary>
		/// If >0, disables flight then decreases itself by one
		/// </summary>
		private int cannotFlyTick = 0;

		/// <summary>
		/// Increased by one every tick the local players y velocity is negative
		/// </summary>
		private int fallingTick = 0;
		private float dtFake = 0;

		// Variables related to Velocity
		private Vector3 finalVelocity; // Modify this value instead of the player's velocity directly, then run `setFinalVelocity = true`
		private bool setFinalVelocity;
		private Vector3 newVelocity; // tmp var
		private Vector3 targetVelocity; // tmp var, usually associated with slerping/lerping
		private float downThrust = 0f;
		private float flapAirFriction = 0.04f; // Prevents the gain of infinite speed while flapping. Set to 0 to remove this feature. THIS IS NOT A MAX SPEED

		// Variables related to gliding
		internal Vector3 wingDirection;
		private float glideDelay = 0; // Minus one per tick, upon hitting ten gliding will gradually come into effect. Zero means gliding functions fully

		// "old" values are the world's defaults (recorded immediately before they are modified)
		private float oldGravityStrength;
		private float oldWalkSpeed;
		private float oldRunSpeed;
		private float oldStrafeSpeed;

		// Avatar-specific properties
		private const HumanBodyBones RightUpperArmBone = HumanBodyBones.RightUpperArm;
		private const HumanBodyBones LeftUpperArmBone = HumanBodyBones.LeftUpperArm;
		private const HumanBodyBones RightLowerArmBone = HumanBodyBones.RightLowerArm;
		private const HumanBodyBones LeftLowerArmBone = HumanBodyBones.LeftLowerArm;
		private const HumanBodyBones RightHandBone = HumanBodyBones.RightHand;
		private const HumanBodyBones LeftHandBone = HumanBodyBones.LeftHand;
		private float shoulderDistance = 0; // Distance between the two shoulders

		[HideInInspector]
		public float armspan = 1f;

		[Tooltip("Default avatar wingtipOffset. (Default: 0)")]
		public float wingtipOffset = 0;

		[Tooltip("Default avatar weight. (Default: 1)")]
		[Range(0f, 2f)]
		public float weight = 1.0f;

        [Tooltip("Limits how often a user can flap, for realism and audio clipping reasons (Default: 0.5)")]
        public float flapdelay = 0.5f;

        public void Start()
        {
            LocalPlayer = Networking.LocalPlayer;
            //save the user gravity if dynamic gravity is disabled
            if (!FP.dynamicPlayerPhysics)
            {
                oldGravityStrength = LocalPlayer.GetGravityStrength();
                oldWalkSpeed = LocalPlayer.GetWalkSpeed();
                oldRunSpeed = LocalPlayer.GetRunSpeed();
                oldStrafeSpeed = LocalPlayer.GetStrafeSpeed();
                Logger.Log("Player Physics saved.", this);
            }
        }

        public void OnEnable()
        {
            timeTick = -20;
            FP.isFlapping = false;
            FP.isFlying = false;
            FP.isGliding = false;
            //spinningRightRound = false;
        }

        public void OnDisable()
        {
            if (FP.isFlying)
            {
                Land();
            }
            Logger.Log("Disabled.", this);
        }

        public override void OnAvatarEyeHeightChanged(VRCPlayerApi player, float eyeHeight) // According to the docs, this also runs upon changing avatars
        {
            if (player == LocalPlayer)
            {
                // Bug: if avatar has been swapped, sometimes the player will be launched straight up.
                // Fix: while cannotFlyTick > 0, do not allow flying. Decreases by one each tick.
                cannotFlyTick = 20;
                setFinalVelocity = false;

                CalculateStats();
            }
        }

        private bool holdingJump = false;
		private int cannotFlapTick = 0;
		private int jumpTick = 0;
		private bool wasGroundedBeforeInputJump = false;
		

        public override void InputJump(bool value, VRC.Udon.Common.UdonInputEventArgs args)
        {
			// ensure every press triggers at least one MainFlightTick tick.
			if (jumpTick <= 0)
			{
				jumpTick = 1;
				wasGroundedBeforeInputJump = LocalPlayer.IsPlayerGrounded();
			}
            holdingJump = value;
        }

        public void Update()
        {
            //do a sanity check due to https://feedback.vrchat.com/udon/p/update-is-executed-for-one-frame-after-the-script-is-disabled
            //we might run for 1 extra frame if we are turned off
            if (!gameObject.activeSelf || !gameObject.activeInHierarchy)
            {
                //return as we shouldnt actually be running any of this code!
                return;
            }

            // FixedUpdate()'s tick rate varies per VR headset.
            // Therefore, I am using Update() to create my own fake homebrew FixedUpdate()
            // It is called MainFlightTick()
            if ((LocalPlayer != null) && LocalPlayer.IsValid())
            {
                dtFake += Time.deltaTime;
                if (dtFake >= DeltaTimeTicksPerSecond)
                {
                    dtFake -= DeltaTimeTicksPerSecond;
                    MainFlightTick(DeltaTimeTicksPerSecond);
                }
            }

        }

        private void MainFlightTick(float fixedDeltaTime)
		{
			if (timeTick < 0)
			{
				// This block only runs once shortly after joining the world. (1/2)
				CalculateStats();
			}

			//Desktop Flight's spacebar checks
			if (cannotFlapTick > 0)
			{
				cannotFlapTick -= 1;
			}
			downThrust = 0;
			// The InputJump() function sets jumpTick to 1, just to ensure every press registers.
			// Hence why we're checking with jumpTick rather than holdingJump here.
			if (jumpTick >= 1)
			{
				if (jumpTick == 1 && (FP.requireJump ? !wasGroundedBeforeInputJump : true))
				{
					if (cannotFlapTick <= 0)
					{
						downThrust = DesktopFlapStrengthMod;
                        cannotFlapTick = (int)Mathf.Round(flapdelay / DeltaTimeTicksPerSecond);
					}
				}
				handsOut = true;
				jumpTick += 1;
			}
			// Essentially an else statement. See prior comment about "The InputJump()..."
			if (!holdingJump)
			{
				jumpTick = 0;
				handsOut = false;
				handsOpposite = false;
			}
			
			if (jumpTick > flapdelay / 5 / DeltaTimeTicksPerSecond)
			{
				handsOpposite = true;
			}
			

			// Only affect velocity this tick if setFinalVelocity == true by the end
			setFinalVelocity = false;

			Vector3 playerPos = LocalPlayer.GetPosition();

			// Check if hands are being moved downward while above a certain Y threshold
			// We're using LocalPlayer.GetPosition() to turn these global coordinates into local ones
			//VRCPlayerApi.TrackingData leftHandData = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
			//RHPos = playerPos - leftHandData.position;
			//RHRot = leftHandData.rotation;

			//VRCPlayerApi.TrackingData rightHandData = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
			//LHPos = playerPos - rightHandData.position;
			//LHRot = rightHandData.rotation;

			if (timeTick < 0)
			{
				// This block only runs once shortly after joining the world. (2/2)
				timeTick = 0;
				//RHPosLast = RHPos;
				//LHPosLast = LHPos;
			}

			//downThrust = 0;

			// Check if player is falling
			if ((!LocalPlayer.IsPlayerGrounded()) && LocalPlayer.GetVelocity().y < 0)
			{
				fallingTick++;
			}
			else
			{
				fallingTick = 0;
			}


			if (!FP.isFlapping)
			{
				// Check for the beginning of a flap
				if (
					(FP.isFlying || handsOut)
					&& (FP.requireJump ? !LocalPlayer.IsPlayerGrounded() : true)
					&& !FP.IsPlayerInStation()
					&& downThrust > 0.002f
				)
				{
					FP.isFlapping = true;
					// TakeOff() will check !isFlying
					TakeOff();
				}
			}

			// This should not be an else. It can trigger the same tick as "if (!isFlapping)"
			if (FP.isFlapping)
			{
				FlapTick();
			}

			// See fallToGlide tooltip
			if (FP.fallToGlide && fallingTick >= FP.fallToGlideActivationDelay && FP.canGlide)
			{
				TakeOff();
			}

			// Flying starts when a player first flaps and ends when they become grounded
			if (FP.isFlying)
			{
				FlyTick(fixedDeltaTime);
			}

			//RHPosLast = RHPos;
			//LHPosLast = LHPos;

			if (cannotFlyTick > 0)
			{
				setFinalVelocity = false;
				cannotFlyTick--;
			}

			if (setFinalVelocity)
			{
				// Hard cap velocity to prevent lag abuse
				finalVelocity = Vector3.ClampMagnitude(finalVelocity, 2000);
				LocalPlayer.SetVelocity(finalVelocity);
			}
		}

        /// <summary>
        /// Flying starts when a player first flaps and ends when they become grounded
        /// </summary>
        /// <param name="dt"></param>
        private void FlyTick(float dt)
        {
            // Check if FlyTick should be skipped this tick
			if (FP.IsMainMenuOpen() || ((!FP.isFlapping) && LocalPlayer.IsPlayerGrounded()))
			{
				Land();
			}
			else
			{
				// Ensure Gravity is correct
				if (LocalPlayer.GetGravityStrength() != FP.GetFlightGravity() && LocalPlayer.GetVelocity().y < 0)
				{
					LocalPlayer.SetGravityStrength(FP.GetFlightGravity());
				}

				// Check for a gliding pose
				// Verbose explanation: (Ensure you're not flapping) && (check for handsOut frame one, ignore handsOut afterwards) && Self Explanatory && Ditto
				if ((!FP.isFlapping) && (FP.isGliding || handsOut) && handsOpposite && FP.canGlide)
				{
					// Currently, glideDelay is being disabled to alleviate a VRChat issue where avatars may spazz out while moving at high velocities.
					// However, this may reintroduce an old bug so we're keeping this here.
					// If gliding is suddenly causing you to bank up and down rapidly, uncomment this:
					// if (LocalPlayer.GetVelocity().y > -1f && (!isGliding)) {glideDelay = 3;}

					FP.isGliding = true;
					newVelocity = setFinalVelocity ? finalVelocity : LocalPlayer.GetVelocity();

					if (glideDelay <= 1)
					{
						Vector3 newForwardRight = Quaternion.Euler(FP.glideAngleOffset, 0, 0) * Vector3.forward;
						Vector3 newForwardLeft = Quaternion.Euler(-FP.glideAngleOffset, 0, 0) * Vector3.forward;

                        Quaternion headrot = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
						// wingDirection is a normal vector pointing towards the forward direction, based on arm/wing angle
						//wingDirection = Vector3.Normalize(LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * newForward);
                        //wingDirection = Vector3.Normalize(Vector3.Slerp(RHRot * newForwardRight, LHRot * newForwardLeft, 0.5f));
                        wingDirection = Vector3.Normalize(headrot * Vector3.forward);
					}
					else
					{
						wingDirection = newVelocity.normalized;
						glideDelay -= 5 * dt;
					}

					// Bug: In rare cases (more common with extremely small avatars) a player's velocity is perfectly straight up/down, which breaks gliding
					// Fix: Always have some form of horizontal velocity while falling.
					if (newVelocity.y < 0.3f && newVelocity.x == 0 && newVelocity.z == 0)
					{
						Vector2 tmpV2 = new Vector2(wingDirection.x, wingDirection.z).normalized * 0.145f;
						newVelocity = new Vector3(Mathf.Round(tmpV2.x * 10) / 10, newVelocity.y, Mathf.Round(tmpV2.y * 10) / 10);
					}

					//steering = (RHPos.y - LHPos.y) * 80 / armspan;
					//clamp steering to 45 degrees
					//steering = Mathf.Clamp(steering, -45, 45);

                    // Fallback "banking" which is just midair strafing. Nobody likes how this feels, should depreciate it
                    //wingDirection = Quaternion.Euler(0, steering, 0) * wingDirection;

					// Favoring Fun over Realism
					// Verbose: X and Z are purely based on which way the wings are pointed ("forward") instead of calculating how the wind would hit each wing, for ease of VR control
					targetVelocity = Vector3.ClampMagnitude(newVelocity + (Vector3.Normalize(wingDirection) * newVelocity.magnitude), newVelocity.magnitude);

					float newGlideControl = (FP.useAvatarModifiers && weight > 1) ? FP.glideControl - ((weight - 1) * 0.6f) : FP.glideControl;
					if (glideDelay > 0)
					{
						glideDelay -= 5 * dt;
					}
					newGlideControl *= (1 - glideDelay) / 1;

					finalVelocity = Vector3.Slerp(newVelocity, targetVelocity, dt * newGlideControl);

					// Apply Air Friction
					finalVelocity *= 1 - (FP.airFriction * 0.011f);
					setFinalVelocity = true;
				}
				else // Not in a gliding pose?
				{
					FP.isGliding = false;
					glideDelay = 0;
				}
			}
        }

        /// <summary>
        /// Flapping starts when a player first flaps and ends when they stop flapping. FlapTick will run every tick.
        /// </summary>
        private void FlapTick()
        {
            if (downThrust > 0)
			{
				// Calculate force to apply based on the flap
				newVelocity = 0.011f * FP.GetFlapStrength() * DesktopFlapStrengthMod * new Vector3(0, 1.0f, 0);

				if (!FP.useAvatarScale)
				{
					//scale up the flap strength by the avatar's size inversely
					// 1 / 0.1 = 10 Smaller than normal Avatar
					// 1 / 1 = 1 Normal
					// 1 / 10 = 0.1 Larger than normal Avatar
					newVelocity = newVelocity / armspan;
				}

				if (LocalPlayer.IsPlayerGrounded())
				{
					// Prevents skiing along the ground
					newVelocity = new Vector3(0, newVelocity.y, 0);
				}
				else
				{
					newVelocity.Scale(new Vector3(FP.horizontalStrengthMod, 1, FP.horizontalStrengthMod));
				}
				finalVelocity = LocalPlayer.GetVelocity() + newVelocity;
				// Speed cap (check, then apply flapping air friction)
				if (finalVelocity.magnitude > 0.02f * FP.GetFlapStrength())
				{
					finalVelocity = finalVelocity.normalized * (finalVelocity.magnitude - (flapAirFriction * FP.GetFlapStrength() * 0.011f));
				}
				setFinalVelocity = true;
			}
			else
			{
				if (FP.IsPlayerInStation())
				{
					finalVelocity = Vector3.zero;
					setFinalVelocity = true;
				}
				FP.isFlapping = false;
			}
        }

        public void FixedUpdate()
        {
            if (timeTick >= 0)
            {
                timeTick++;
                // Automatically update the debug output every 0.2 seconds (sorta, since certain VR headsets affect FixedUpdate())
                if (timeTick > 9)
                {
                    timeTick = 0;
                    if (FP.debugOutput != null)
                    {
                        //Don't add tabs back here, or else they will end up in the multiline string
                        FP.debugOutput.text = string.Format(
@"Is Player Flying: {0}
Is Player Flapping: {1}
Is Player Gliding: {2}
--Internal Vars--
Hands Out: {3}
Downward Thrust: {4}
Cannot Fly: {5}
Glide Delay: {6}
--Player Controller State--
Grounded: {7}
Velocity: {8}",
                            FP.isFlying,
                            FP.isFlapping,
                            FP.isGliding,
                            false, // Hands are never out on Desktop
                            downThrust,
                            cannotFlyTick > 0,
                            glideDelay,
                            LocalPlayer.IsPlayerGrounded(),
                            LocalPlayer.GetVelocity()
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Immobilizes the player's locomotion. This is useful for preventing the player from moving while flying. Still allows the player to rotate, unlike VRC's method of immobilization.
        /// </summary>
        /// <param name="immobilize"></param>
        private void ImmobilizePlayer(bool immobilize)
        {
            //This is non-zero as it allows the "forward" direction of a player to still update while flying, fixing some animation bugs.
            const float ImmobileSpeed = 0.001f;

            if (immobilize)
            {
                if (FP.dynamicPlayerPhysics)
                {
                    oldWalkSpeed = LocalPlayer.GetWalkSpeed();
                    oldRunSpeed = LocalPlayer.GetRunSpeed();
                    oldStrafeSpeed = LocalPlayer.GetStrafeSpeed();
                }

                LocalPlayer.SetWalkSpeed(ImmobileSpeed);
                LocalPlayer.SetRunSpeed(ImmobileSpeed);
                LocalPlayer.SetStrafeSpeed(ImmobileSpeed);
            }
            else
            {
                LocalPlayer.SetWalkSpeed(oldWalkSpeed);
                LocalPlayer.SetRunSpeed(oldRunSpeed);
                LocalPlayer.SetStrafeSpeed(oldStrafeSpeed);
            }
        }

        /// <summary>
        /// Running this function will recalculate important variables needed for Flap Strength.
        /// </summary>
        private void CalculateStats()
        {
            // `armspan` does not include the distance between shoulders. shoulderDistance stores this value by itself.
            armspan =
                Vector3.Distance(LocalPlayer.GetBonePosition(LeftUpperArmBone), LocalPlayer.GetBonePosition(LeftLowerArmBone))
                + Vector3.Distance(LocalPlayer.GetBonePosition(LeftLowerArmBone), LocalPlayer.GetBonePosition(LeftHandBone))
                + Vector3.Distance(LocalPlayer.GetBonePosition(RightUpperArmBone), LocalPlayer.GetBonePosition(RightLowerArmBone))
                + Vector3.Distance(LocalPlayer.GetBonePosition(RightLowerArmBone), LocalPlayer.GetBonePosition(RightHandBone));
            shoulderDistance = Vector3.Distance(LocalPlayer.GetBonePosition(LeftUpperArmBone), LocalPlayer.GetBonePosition(RightUpperArmBone));
            Logger.Log("Armspan: " + armspan.ToString() + " Shoulder Distance: " + shoulderDistance.ToString(), this);
        }

        /// <summary>
        /// Set necessary values for beginning flight. Automatically ensures it only runs on the first tick of flight.
        /// </summary>
        public void TakeOff()
        {
            if (!FP.isFlying)
            {
                FP.isFlying = true;
                if (FP.dynamicPlayerPhysics)
                {
                    oldGravityStrength = LocalPlayer.GetGravityStrength();
                }
                else
                {
                    CheckPhysicsUnchanged();
                }
                LocalPlayer.SetGravityStrength(FP.GetFlightGravity());
                if (!FP.allowLoco)
                {
                    //ImmobilizePlayer(true);
                }
                Logger.Log("Took off.", this);
            }
        }

        /// <summary>
        /// Checks if the world gravity or player movement has changed from the saved values and throws a warning if so.
        /// </summary>
        private void CheckPhysicsUnchanged()
        {
            // Log a warning if gravity values differ from what we have saved
            if (LocalPlayer.GetGravityStrength() != oldGravityStrength)
            {
                Logger.LogWarning(
                    "World gravity is different than the saved gravity, this may cause issues. If you want to avoid this, edit scripts to inform OpenFlight of the new world gravity using UpdatePlayerPhysics().",
                    this
                );
                Logger.LogWarning("Saved Gravity: " + oldGravityStrength.ToString(), this);
            }

            // Log a warning if movement values differ from what we have saved
            if (LocalPlayer.GetWalkSpeed() != oldWalkSpeed || LocalPlayer.GetRunSpeed() != oldRunSpeed || LocalPlayer.GetStrafeSpeed() != oldStrafeSpeed)
            {
                Logger.LogWarning(
                    "Player movement is different than the saved movement, this may cause issues. If you want to avoid this, edit scripts to inform OpenFlight of the new player movement using UpdatePlayerPhysics().",
                    this
                );
                Logger.LogWarning(
                    "Saved Walk Speed: " + oldWalkSpeed.ToString() + " Saved Run Speed: " + oldRunSpeed.ToString() + " Saved Strafe Speed: " + oldStrafeSpeed.ToString(),
                    this
                );
            }
        }

        /// <summary>
        /// Effectually disables all flight-related variables and functions. This does not permanently disable flight (the player can just flap again); disable the GameObject instead.
        /// </summary>
        public void Land()
        {
            FP.isFlying = false;
            FP.isFlapping = false;
            FP.isGliding = false;
            LocalPlayer.SetGravityStrength(oldGravityStrength);

            if (!FP.allowLoco)
            {
                //ImmobilizePlayer(false);
            }

            if (!FP.dynamicPlayerPhysics)
            {
                CheckPhysicsUnchanged();
            }

            Logger.Log("Landed.", this);
        }

        /// <summary>
        /// Calling this function tells the script to pull in the worlds values for player physics. This is useful if you have a world that changes gravity or movement often, but still want water systems to work.
        /// </summary>
        /// <remarks>
        /// This function is only useful if dynamic player physics is disabled. Otherwise, it will do nothing.
        /// </remarks>
        public void UpdatePlayerPhysics()
        {
            if (FP.dynamicPlayerPhysics)
            {
                Logger.Log("Dynamic Player Physics is enabled. Player Physics will be updated automatically.", this);
                return;
            }

            oldGravityStrength = LocalPlayer.GetGravityStrength();
            oldWalkSpeed = LocalPlayer.GetWalkSpeed();
            oldRunSpeed = LocalPlayer.GetRunSpeed();
            oldStrafeSpeed = LocalPlayer.GetStrafeSpeed();
            Logger.Log("Player Physics updated.", this);
        }
    }
}