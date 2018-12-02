using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectChangeEffect : MonoBehaviour
{
    public bool IsAutomatic;
    public RectTransform Gadget;
    public float Speed;
    public Vector2 Padding;

    private Vector3 TargetPos;
    private Vector2 TargetSize;
    
    private List<CustomButton> _selectables;

    public void Start()
    {
        if (!IsAutomatic)
            return;
        
        if (_selectables != null)
            _selectables.Clear();
        else
            _selectables = new List<CustomButton>();
        
        _selectables.AddRange(GetComponentsInChildren<CustomButton>(true));

        foreach (CustomButton sth in _selectables)
        {
            var lambda = BuildLambda(sth);
            sth.OnSelected.RemoveListener(lambda);
            sth.OnSelected.AddListener   (lambda);
        }
    }

    private UnityAction BuildLambda(Selectable slct)
    {
        return () => { OnSelectionChanged(slct); };
    }

    private void OnSelectionChanged(Selectable slct)
    {
        var rect = slct.GetComponent<RectTransform>();
        TargetPos = rect.position;
        TargetSize = rect.sizeDelta + Padding;
    }

    void Update()
    {
        if (!Gadget) return;
        
        Gadget.position = Vector3.Lerp(Gadget.position, TargetPos, Time.unscaledDeltaTime * Speed);
        Gadget.sizeDelta = Vector2.Lerp(Gadget.sizeDelta, TargetSize, Time.unscaledDeltaTime * Speed);
    }

    public void ForceSelect(Selectable selectable)
    {
        selectable.Select();
        OnSelectionChanged(selectable);
    }

    public void HideGadget()
    {
        TargetPos  = Vector3.zero;
        TargetSize = Vector2.zero;
    }
}
