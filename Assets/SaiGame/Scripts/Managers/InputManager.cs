using UnityEngine;

public class InputManager : SaiSingleton<InputManager>
{
    [SerializeField] protected bool isAiming = false;

    [SerializeField] protected bool isAttackLight = false;
    [SerializeField] protected bool isAttackHeavy = false;
    [SerializeField] protected bool airmToogle = false;
    [SerializeField] protected bool isToogleInventory = false;
    public bool IsToogleInventory => isToogleInventory;

    private void Update()
    {
        this.CheckAiming();
        this.CheckAttacking();
        this.OpenInventory();
    }

    protected virtual void OpenInventory()
    {
        this.isToogleInventory = Input.GetKeyUp(KeyCode.I);
    }

    protected virtual void CheckAiming()
    {
        //this.isAiming = Input.GetMouseButton(1);

        if (Input.GetKeyUp(KeyCode.LeftControl)) this.airmToogle = !this.airmToogle;
        this.isAiming = this.airmToogle;
    }

    protected virtual void CheckAttacking()
    {
        if (!this.IsAiming())
        {
            this.isAttackHeavy = false;
            this.isAttackLight = false;
            return;
        }

        this.isAttackLight = Input.GetMouseButton(0);
        this.isAttackHeavy = Input.GetMouseButton(1);

    }

    public virtual bool IsAttackLight()
    {
        return this.isAttackLight;
    }

    public virtual bool IsAttackHeavy()
    {
        return this.isAttackHeavy;
    }

    public virtual bool IsAiming()
    {
        return this.isAiming;
    }
} 