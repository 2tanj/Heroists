using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable enable
[RequireComponent(typeof(SpriteRenderer))]
public abstract class Unit : IAudioPlayer, IHoverable // IAudioPlayer has MonoBehaviour
{
    [SerializeField] private AudioClip _moveSound      ;
    [SerializeField] private AudioClip _attackSound    ;
    [SerializeField] private AudioClip _damageTakeSound;
    [SerializeField] private AudioClip _onDeathSound   ;

    public Node ?Position { get; private set; }

    [field: SerializeField, Header("Unit Information")]
    internal UnitStats _statsReference;
    public UnitStats Stats { get; internal set; }

    private float _health;
    public float Health { 
        get => _health; 
        set {
            AnimateSliderOnDamage();

            if (value < _health)
            {
                Debug.Log(Stats.Name + " just took damage!");
                Debug.Log("Old HP: " + _health);
                Debug.Log("New HP: " + value);

                GUIManager.Instance.SpawnDamageNumber(transform.position, _health - value, true);

                PlaySound(_damageTakeSound);
                transform.DOShakePosition(.1f, .1f, 1);
            }
            
            _health = Mathf.Clamp(value, -1, Stats.MaxHealth);
            if (_hpSlider != null)
                _hpSlider.value = _health;

            if (_health <= 0)
                Die();
        } 
    }

    public IAbility? OffensiveAbility { get; internal set; }
    public IAbility? DefensiveAbility { get; internal set; }

    private bool _abilityPerformed = false;
    public UnityAction OnAbilityPerformed;

    #region UI
    private Slider _hpSlider;
    private void SetupHPSlider()
    {
        _hpSlider = GetComponentInChildren<Slider>();

        _hpSlider.maxValue = Stats.MaxHealth;
        _hpSlider.value    = Health;
    }
    private void AnimateSliderOnDamage()
    {
        if (_hpSlider == null)
            return;

        _hpSlider.transform.DOPunchScale(new Vector3(1, 1), .2f);
    }
    #endregion

    private void Start()
    {
        Health = Stats.MaxHealth;

        SetupHPSlider();
        SetupAudio();

        OnAbilityPerformed += () => _abilityPerformed = true;
    }

    public void ShowInfo()
    {
        GUIManager.Instance.ShowInfoGUI(
            GetComponent<SpriteRenderer>().sprite, Stats.Name, Stats.Description);

        GUIManager.Instance.ShowSelectedUnitInfo(this);
    }

    public abstract void PerformAbility(AbilityType? type, Node? n);

    public IEnumerator WaitUntilAbilityIsPerformed()
    {
        // moving the camera to the unit thats about to move
        float moveCameraDuration = .8f;
        Camera.main.transform.DOMove(
            new Vector3(transform.position.x,
                        transform.position.y,
                        Camera.main.transform.position.z), 
            moveCameraDuration);
        yield return new WaitForSeconds(moveCameraDuration);

        _abilityPerformed = false;
        if      (this is HeroUnit)
        {
            GUIManager.Instance.SetupAbilityButtons((HeroUnit)this);
            GameManager.Instance.GetValidMoves();
        }
        else if (this is EnemyUnit)
            PerformAbility(null, null);

        yield return new WaitUntil(() => _abilityPerformed);
        _abilityPerformed = false;
    }

    public static Unit CreateUnit(Unit unitPrefab, Node unitPos)
    {
        var unit = Instantiate(unitPrefab, unitPos.transform.position, Quaternion.identity);
        unit.gameObject.SetActive(true);
        unit.Move(unitPos);

        return unit;
    }

    public void Move(Node newNode)
    {
        PlaySound(_moveSound);

        if (Position != null)
            Position.Unit = null;
        
        if (newNode.Pickup != null)
        {
            PlaySound(newNode.Pickup.Sound);

            newNode.Pickup.UsePickup(this);
        }

        transform.DOMove(newNode.transform.position, .28f).SetEase(Ease.OutQuart).OnComplete(() => {
            newNode.Unit   = this;

            Position = newNode;
            transform.SetParent(newNode.transform);

            PlaySound(_moveSound);


            OnAbilityPerformed?.Invoke();

            // we do this again since the call above in PerformAbility
            // executes instantly(and the valid moves dont change)
            // because of the animation(the unit is still moving towards newNode)
            //GameManager.Instance.GetValidMoves();
        });
    }

    public void Attack(Unit target)
    {
        if (target == null || Stats.Team == target.Stats.Team)
            return;

        PlaySound(_attackSound);

        float punchDuration = .45f;
        transform.DOPunchPosition(
            punch: target.transform.position - transform.position,
            duration: punchDuration,
            vibrato: 7,
            elasticity: .1f
        ).OnComplete(() => OnAbilityPerformed?.Invoke());

        // perform the attack logic half way through the animation
        // (when the unit touches the target)
        StartCoroutine(WaitAndDo(punchDuration / 3, 
            () => DoDamage(target, Stats.AttackDamage)));
    }

    public void DoDamage(Unit target, float damage) =>
        target.Health -= damage - (damage * target.Stats.Defense);

    // TODO: add SFX
    public void Die()
    {
        // doesnt work for some reason :))
        PlaySound(_onDeathSound);

        Debug.Log(Stats.Name + " died. :(");
        Destroy(gameObject);
    }

    internal IEnumerator WaitAndDo(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action.Invoke();
    }
}