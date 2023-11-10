using FunkyCode.SuperTilemapEditorSupport.Light.Shadow;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;
using static Pieta;

public class Pieta : MonoBehaviour
{
    
    private List<MarkedTarget> MarkedTargets;

    public GameObject HintPrefab;
    public Sprite[] HintSprites;

    public static Pieta i;

    [SerializeField]
    private float farDis, nearDis;

    private PlayerMain Player;

    public List<MarkedTarget> CanPietaList;
    public float PietaEndDis;

    private void Awake()
    {
        i = this;
        Player = GetComponent<PlayerMain>();
        MarkedTargets = new List<MarkedTarget>();
        CanPietaList = new List<MarkedTarget>();
        PietaEndDis = 0f;
    }

    private void Update()
    {
        MarkNearbyEnemys();
    }

    private RaycastHit2D[] CanPietaRange()
    {
        BoxCollider2D component = Player.GetComponent<BoxCollider2D>();
        RaycastHit2D[] raycastHit2D = Physics2D.BoxCastAll(component.bounds.center, component.size, 0f, Vector2.right * Player.Facing, nearDis / 2, LayerMask.GetMask("Character"));

        Debug.DrawLine(component.bounds.center, component.bounds.center + nearDis / 2 * Vector3.right * Player.Facing, Color.yellow);

        return raycastHit2D;
    }

    public SpriteRenderer Appear(Transform target, GameObject HintPrefab)
    {
        return Instantiate(HintPrefab, target.position + (Vector3)target.GetComponent<Character>().MarkPos, Quaternion.identity, target).GetComponent<SpriteRenderer>();
    }

    public void Disappear(MarkedTarget markedTarget)
    {
        MarkedTargets.Remove(markedTarget);
        Destroy(markedTarget.Hint.gameObject);
    }

    public void MarkNearbyEnemys()
    {
        Vector2 Range = Vector2.one* farDis;

        Vector3 topRight = Range / 2;
        Vector3 topLeft = new Vector2(-Range.x, Range.y) / 2;
        Vector3 bottomRight = new Vector2(Range.x, -Range.y) / 2;
        Vector3 bottomLeft = -Range / 2;

        Debug.DrawLine(Player.transform.position + topRight, Player.transform.position + topLeft, Color.magenta);
        Debug.DrawLine(Player.transform.position + topRight, Player.transform.position + bottomRight, Color.magenta);
        Debug.DrawLine(Player.transform.position + topLeft, Player.transform.position + bottomLeft, Color.magenta);
        Debug.DrawLine(Player.transform.position + bottomRight, Player.transform.position + bottomLeft, Color.magenta);

        Vector2 Range2 = Vector2.one * nearDis;

        Vector3 topRight2 = Range2 / 2;
        Vector3 topLeft2 = new Vector2(-Range2.x, Range2.y) / 2;
        Vector3 bottomRight2 = new Vector2(Range2.x, -Range2.y) / 2;
        Vector3 bottomLeft2 = -Range2 / 2;

        Debug.DrawLine(Player.transform.position + topRight2, Player.transform.position + topLeft2, Color.yellow);
        Debug.DrawLine(Player.transform.position + topRight2, Player.transform.position + bottomRight2, Color.yellow);
        Debug.DrawLine(Player.transform.position + topLeft2, Player.transform.position + bottomLeft2, Color.yellow);
        Debug.DrawLine(Player.transform.position + bottomRight2, Player.transform.position + bottomLeft2, Color.yellow);

        Collider2D[] far = Physics2D.OverlapBoxAll(Player.transform.position, Vector2.one * farDis, 0f, LayerMask.GetMask("Character"));
        Collider2D[] near = Physics2D.OverlapBoxAll(Player.transform.position, Vector2.one * nearDis, 0f, LayerMask.GetMask("Character"));

        //��S�O���L���[�JMarkTargets
        foreach (Collider2D target in far)
        {
            if (target == Player.Collider)
                continue;

            bool hasTarget = false;
            foreach (MarkedTarget markedTarget in MarkedTargets)
            {
                if (markedTarget.Collider2D == target) 
                {
                    hasTarget = true;
                    break;
                }
            }
            if(!hasTarget) MarkedTargets.Add(new MarkedTarget(target, Appear(target.transform, HintPrefab)));
        }

        RaycastHit2D[] FacingTargets = CanPietaRange();

        List<MarkedTarget> disappearList = new List<MarkedTarget>();

        //�����}�d�򤺪��аO�R���B��s�аO�ϥ�
        foreach (MarkedTarget markedTarget in MarkedTargets)
        {
            if (!far.Contains(markedTarget.Collider2D))
            {
                disappearList.Add(markedTarget);
                continue;
            }

            if (near.Contains(markedTarget.Collider2D))
            {
                bool Facing = false;
                if (FacingTargets.Length > 1)//���u�g��ۤv
                {
                    foreach (RaycastHit2D FacingTarget in FacingTargets)
                    {
                        if (FacingTarget.collider == markedTarget.Collider2D)
                        {
                            Facing = true;
                            break;
                        }
                    }
                }

                if (Facing)
                    //�iĲ�o
                    markedTarget.Hint.sprite = HintSprites[2];
                else
                    //��
                    markedTarget.Hint.sprite = HintSprites[1];
            }
            else
            {
                //��
                markedTarget.Hint.sprite = HintSprites[0];
            }
        }

        foreach (MarkedTarget markedTarget in disappearList)
        {
            Disappear(markedTarget);
        }
    }

    public bool CheckPietaAttack()
    {
        CanPietaList = new List<MarkedTarget>();

        //���Ҧ��i�t�ܥؼ�
        foreach (MarkedTarget markedTarget in MarkedTargets)
        {
            if (markedTarget.Hint.sprite == HintSprites[2])
            {
                markedTarget.SlicePos = markedTarget.Collider2D.transform.position;
                CanPietaList.Add(markedTarget);
            }
        }

        PietaEndDis = 0f;

        if (CanPietaList.Count <= 0)
            return false;

        //���̻��i�t�ܥؼжZ��
        foreach (MarkedTarget markedTarget in CanPietaList)
        {
            float newDis = Vector2.Distance(Player.transform.position, markedTarget.Collider2D.transform.position);
            if (PietaEndDis < newDis) 
            {
                PietaEndDis = newDis;
            }
        }

        return true;
    }
}

public class MarkedTarget
{
    public Collider2D Collider2D;

    public SpriteRenderer Hint;

    public Vector2 SlicePos;

    public MarkedTarget(Collider2D _collider2D, SpriteRenderer _Hint)
    {
        Collider2D = _collider2D;
        Hint = _Hint;
    }
}
