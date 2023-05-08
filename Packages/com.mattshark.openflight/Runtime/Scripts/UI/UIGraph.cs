using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace OpenFlightVRC.UI
{
	public class UIGraph : UdonSharpBehaviour
	{
		AnimationCurve curve; // The curve to graph
		LineRenderer lineRenderer; // The line renderer component
		RectTransform lineRendererRectTransform; // The transform of the line renderer
		public RectTransform evalPointTransform; // The transform of the evaluation point graphic
		public UdonBehaviour target; // The target UdonBehaviour
		public string targetVariableCurve; // The target variable for the curve
		public string targetVariableEval; // The target variable for the evaluation point
		int resolution = 20; // The number of points to graph

		float normalizationFactor = 0f; //this is used if the curve is not normalized vertically from 0 to 1
		float totalTime = 1.0f; //this is used if the curve is not normalized horizontally from 0 to 1

		void Start()
		{
			//get the real target from the proxy, if it exists
			if (target.GetProgramVariable("target") != null)
				target = (UdonBehaviour)target.GetProgramVariable("target");

			//the line renderer is a child of the UI object
			lineRenderer = GetComponentInChildren<LineRenderer>();

			lineRendererRectTransform = lineRenderer.GetComponent<RectTransform>();

			//set the line renderer to the correct number of points
			lineRenderer.positionCount = resolution + 1;

			//determine if the curve is normalized by checking if any of the numbers at a time are greater than 1
			curve = (AnimationCurve)target.GetProgramVariable(targetVariableCurve);

			//step through the curve backwards till the value that is evaluated has changed
			float initialValue = curve.Evaluate(100f);
			for (int i = 100; i > 0; i--)
			{
				float value = curve.Evaluate(i);
				if (value != initialValue)
				{
					totalTime = i;
					break;
				}
			}

			//if the curve is not normalized, then we need to normalize it
			for (int i = 0; i < resolution; i++)
			{
				float time = (float)i / (float)resolution;
				float value = curve.Evaluate(time);
				if (value > normalizationFactor)
				{
					normalizationFactor = value;
				}
			}

			//evaluate the curve at each point and set the line renderer positions
			float elementWidth = lineRendererRectTransform.rect.width;
			float elementHeight = lineRendererRectTransform.rect.height / normalizationFactor;
			//print each point in the curve
			//we need to use curve.Evaluate() to get the value at a specific time since curve.keys[] is not exposed
			for (int i = 0; i < resolution + 1; i++)
			{
				float time = (float)i / (float)resolution;
				float value = curve.Evaluate(time * totalTime);
				lineRenderer.SetPosition(i, new Vector3(time * elementWidth, value * elementHeight, 0));
			}
		}

		void Update()
		{
			//place the evaluation point at the correct position
			float evalTime = (float)target.GetProgramVariable(targetVariableEval);

			evalPointTransform.anchoredPosition = new Vector2(
				(evalTime / (totalTime + 1)) * lineRendererRectTransform.rect.width,
				curve.Evaluate(evalTime) * lineRendererRectTransform.rect.height / normalizationFactor
			);
		}
	}
}
