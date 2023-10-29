using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActionTeleport", menuName = "Actions/Teleport")]
public class ActionTeleportObj : ActionBaseObj
{
    private Vector2 TeleportTargetPos;

    public override ActionPeformState StartAction(Character _m)
    {
        TeleportTargetPos = _m.TeleportKeyReference.value;

        return base.StartAction(_m);
    }

    public override void EndAction(Character _m)
    {
        base.EndAction(_m);

        _m.Rigid.MovePosition(TeleportTargetPos);
    }
}


