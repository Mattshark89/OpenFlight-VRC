using System;
using UdonSharp;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.Contact
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AvatarContacts : CallbackUdonSharpBehaviour
    {
        /// <summary>
		/// The OpenFlight script, used to enable/disable flight
		/// </summary>
		public OpenFlight OpenFlight;

        /// <summary>
        /// The contact sender, for telling avatars that they are flying.
        /// </summary>
        public VRCContactSender Sender;

        public FlightProperties FP;

        public AvatarDetection AviDetect;
        private VRCPlayerApi Localplayer;


        public void Start()
        {
            Sender.enabled = false;
            Localplayer = Networking.LocalPlayer;
        }

        internal void OnFlyingChanged(bool boolState)
        {
            Sender.enabled = boolState;
            Logger.Log("Avatar OF_IsFlying Contact " + boolState, this);
        }

        public override void OnContactEnter(ContactEnterInfo contactInfo)
        {
            if (FP.useAvatarModifiers){
                //Check if local user contact
                if (contactInfo.contactSender.isValid)
                {
                    if (Localplayer == contactInfo.contactSender.player)
                    {
                        // While one reciever is limited to 16 tags, pretty sure just adding another contact reciever to the game object will give us an addition 16 if required.
                        string[] tags = contactInfo.matchingTags;

                        // We can't use string.Contain which requires linq or string.exists which requires lambda functions, Udon doesn't expose either.
                        foreach (var tag in tags)
                        {
                            switch (tag)
                            {
                                case "OF_NotisOn":
                                    //Forcefully turn on notification
                                    FP.notifications = true;
                                    Logger.Log("Notifications turned on using contacts", this);
                                    break;

                                case "OF_NotisOff":
                                    //Forcefully turn off notification
                                    FP.notifications = false;
                                    Logger.Log("Notifications turned off using contacts", this);
                                    break;

                                case "OF_JumpFlyOn":
                                    //Forcefully turn on jump to fly
                                    FP.requireJump = true;
                                    Logger.Log("Require jump turned on using contacts", this);
                                    break;

                                case "OF_JumpFlyOff":
                                    //Forcefully turn off jump to fly
                                    FP.requireJump = false;
                                    Logger.Log("Require jump turned off using contacts", this);
                                    break;
                                
                                case "OF_BankingOn":
                                    //Forcefully turn on banking
                                    FP.bankingTurns = true;
                                    Logger.Log("Banking turned on using contacts", this);
                                    break;

                                case "OF_BankingOff":
                                    //Forcefully turn off banking
                                    FP.bankingTurns = false;
                                    Logger.Log("Banking turned off using contacts", this);
                                    break;

                                case "OF_CanGlideOn":
                                    FP.canGlide = true;
                                    Logger.Log("Gliding turned on using contacts", this);
                                    break;

                                case "OF_CanGlideOff":
                                    FP.canGlide = false;
                                    Logger.Log("Gliding turned off using contacts", this);
                                    break;

                                case "OF_GlideControlMod":
                                    float contactrotX = Math.Abs(contactInfo.contactSender.rotation.eulerAngles.x - this.transform.rotation.eulerAngles.x) + 0.0001f;
                                    Logger.Log("GlideControl Contact Sender Rotation X: " + contactInfo.contactSender.rotation.eulerAngles.x, this);
                                    float newglidecontrol = Mathf.Lerp(1.0f, 4.0f, contactrotX/90);
                                    FP.glideControl = newglidecontrol;
                                    Logger.Log("Glide control set to " + newglidecontrol.ToString() + " using contacts", this);
                                    break;

                                case "OF_GravityMod":
                                    float contactrotY = Math.Abs(contactInfo.contactSender.rotation.eulerAngles.y - this.transform.rotation.eulerAngles.y) + 0.0001f;
                                    Logger.Log("Gravity Contact Sender Rotation Y: " + contactInfo.contactSender.rotation.eulerAngles.y, this);
                                    float newgravity = Mathf.Lerp(0.1f, 1.0f, contactrotY/90);
                                    FP.flightGravityBase = newgravity;
                                    Logger.Log("Flight gravity set to " + newgravity.ToString() + " using contacts", this);
                                    break;

                                case "OF_WeightMod":
                                    float contactrotZ = Math.Abs(contactInfo.contactSender.rotation.eulerAngles.z - this.transform.rotation.eulerAngles.z) + 0.0001f;
                                    Logger.Log("WeightMod Contact Sender Rotation Z: " + contactInfo.contactSender.rotation.eulerAngles.z, this);
                                    float newweight = Mathf.Lerp(0.1f, 10f, contactrotZ/90);
                                    AviDetect.weight = newweight;
                                    Logger.Log("Weight set to " + newweight.ToString() + " using contacts", this);
                                    break;

                                case "OF_WingOffsetMod":
                                    //Tell system that the wing offset is being modified.
                                    float contactX = Math.Abs(contactInfo.contactSender.position.x - this.transform.position.x);
                                    float newoffset = Mathf.Lerp(0.0f, 40.0f, contactX);
                                    AviDetect.WingtipOffset = newoffset;
                                    Logger.Log("Wingoffset set to " + newoffset.ToString() + " using contacts", this);
                                    break;

                                case "OF_FlapStrengthMod":
                                    //Tell system that flap strength is being modified
                                    float contactY = Math.Abs(contactInfo.contactSender.position.y - this.transform.position.y);
                                    int newstrength = (int)Mathf.Lerp(100.0f, 800.0f, contactY);
                                    FP.flapStrengthBase = newstrength;
                                    Logger.Log("Flap strength set to " + newstrength.ToString() + " using contacts", this);
                                    break;

                                case "OF_FrictionMod":
                                    //Tell system that friction is being modified
                                    float contactZ = Math.Abs(contactInfo.contactSender.position.z - this.transform.position.z);
                                    float newfriction = Mathf.Lerp(0.0f, 0.2f, contactZ);
                                    FP.airFriction = newfriction;
                                    Logger.Log("Friction set to " + newfriction.ToString() + " using contacts", this);
                                    break;

                                case "OF_CanFly":
                                    Logger.Log("Contact is activating flying", this);
                                    OpenFlight.CanFly();
                                    break;

                                case "OF_CanNotFly":
                                    Logger.Log("Contact is deactivating flying", this);
                                    OpenFlight.CannotFly();
                                    break;

                                default:
                                    Logger.Log("Unknown contact: " + tag, this);
                                    break;
                            }
                        }
                    }
                }
            }
            else
            {
                Logger.Log("World creator has disabled contact support", this);
            }
        }
    }
}