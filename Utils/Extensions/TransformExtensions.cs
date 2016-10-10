using UnityEngine;

namespace RectTransformExtensions
{
	public static class TransformExtender
	{
		public static bool IsOnRightOf(this RectTransform first, RectTransform second)
		{
			return first.anchoredPosition.x > second.anchoredPosition.x;
		}

		public static bool IsOnLeftOf(this RectTransform first, RectTransform second)
		{
			return first.anchoredPosition.x < second.pivot.x;
		}

		public static bool IsOnCenterOf(this RectTransform first, RectTransform second)
		{
			return Mathf.Abs(first.anchoredPosition.x - second.anchoredPosition.x) < float.Epsilon;
		}
	}
}
