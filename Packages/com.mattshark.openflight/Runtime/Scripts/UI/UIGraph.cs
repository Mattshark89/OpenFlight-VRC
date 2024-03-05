/**
 * @ Maintainer: Happyrobot33
 */

using UdonSharp;
using UnityEngine;

namespace OpenFlightVRC.UI
{
	/// <summary>
	/// A graph that graphs an animation curve
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class UIGraph : UIBase
	{
		private AnimationCurve _curve; // The curve to graph
		private LineRenderer _lineRenderer; // The line renderer component
		private RectTransform _lineRendererRectTransform; // The transform of the line renderer
		public RectTransform EvalPointTransform; // The transform of the evaluation point graphic
		public string targetVariableCurve; // The target variable for the curve
		public string targetVariableEval; // The target variable for the evaluation point
		private const int Resolution = 20; // The number of points to graph

		private float _normalizationFactor = 0f; //this is used if the curve is not normalized vertically from 0 to 1
		private float _totalTime = 1.0f; //this is used if the curve is not normalized horizontally from 0 to 1

		void Start()
		{
			InitializeTargetInfo();
			//the line renderer is a child of the UI object
			_lineRenderer = GetComponentInChildren<LineRenderer>();

			_lineRendererRectTransform = _lineRenderer.GetComponent<RectTransform>();

			//set the line renderer to the correct number of points
			_lineRenderer.positionCount = Resolution + 1;

			//determine if the curve is normalized by checking if any of the numbers at a time are greater than 1
			_curve = (AnimationCurve)target.GetProgramVariable(targetVariableCurve);

			//step through the curve backwards till the value that is evaluated has changed
			float initialValue = _curve.Evaluate(100f);
			for (int i = 100; i > 0; i--)
			{
				float value = _curve.Evaluate(i);
				if (value != initialValue)
				{
					_totalTime = i;
					break;
				}
			}

			//if the curve is not normalized, then we need to normalize it
			for (int i = 0; i < Resolution; i++)
			{
				float time = (float)i / (float)Resolution;
				float value = _curve.Evaluate(time);
				if (value > _normalizationFactor)
				{
					_normalizationFactor = value;
				}
			}

			//evaluate the curve at each point and set the line renderer positions
			float elementWidth = _lineRendererRectTransform.rect.width;
			float elementHeight = _lineRendererRectTransform.rect.height / _normalizationFactor;
			//print each point in the curve
			//we need to use curve.Evaluate() to get the value at a specific time since curve.keys[] is not exposed
			for (int i = 0; i < Resolution + 1; i++)
			{
				float time = (float)i / (float)Resolution;
				float value = _curve.Evaluate(time * _totalTime);
				_lineRenderer.SetPosition(i, new Vector3(time * elementWidth, value * elementHeight, 0));
			}
		}

		void Update()
		{
			//place the evaluation point at the correct position
			float evalTime = (float)target.GetProgramVariable(targetVariableEval);

			EvalPointTransform.anchoredPosition = new Vector2(
				(evalTime / (_totalTime + 1)) * _lineRendererRectTransform.rect.width,
				_curve.Evaluate(evalTime) * _lineRendererRectTransform.rect.height / _normalizationFactor
			);
		}
	}
}
