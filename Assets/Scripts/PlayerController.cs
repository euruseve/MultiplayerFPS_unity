using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform viewPoint;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private GameObject bulletImpact;
    [SerializeField] private LayerMask groundLayers; 
    [SerializeField] private CharacterController charController;

    [SerializeField] private List<Gun> allGuns;

    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float jumpForce = 11f;
    [SerializeField] private float gravityMod = 7f;
//    [SerializeField] private float timeBetweenShots = .1f;
    [SerializeField] private float maxHeat = 10f;
//    [SerializeField] private float heatPerShot = 1f;
    [SerializeField] private float coolRate = 4f;
    [SerializeField] private float overheatCoolRate = 5f;
    [SerializeField] private bool invertLook;

    [SerializeField] private GameObject playerHitImpact;

    [SerializeField] private float muzzleDisplayTime;
    private float _muzzleCounter;

    private int _selectedGun;
    private float _shotCounter;
    private float _verticalRotStore;
    private float _activeMoveSpeed;
    private float _heatCouner;
    private bool _isGrounded;
    private bool _overHeated;

    
    private Vector2 _mouseInput;
    private Vector3 _moveDirection;
    private Vector3 _movement;
    private Camera _cam;

    [SerializeField] private int maxHealth = 100;

    [SerializeField] private Animator anim;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private Transform modelGunPoint;
    [SerializeField] private Transform gunHolder;




    private int _currentHealth;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        UIController.instance.weaponTempSlider.maxValue = maxHeat;
        _cam = Camera.main;

        //SwitchGun();

        photonView.RPC("SetGun", RpcTarget.All, _selectedGun);

        _currentHealth = maxHealth;


        if(photonView.IsMine)
        {
            playerModel.SetActive(false);

            UIController.instance.healthSlider.maxValue = maxHealth;
            UIController.instance.healthSlider.value = _currentHealth;
        }
        else{
            gunHolder.parent = modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }

        // Transform newTrans = SpawnManager.instance.GetSpawnPoint();
        // transform.position = newTrans.position;
        // transform.rotation = newTrans.rotation;
    }


    private void LateUpdate() {

        if(photonView.IsMine)
        {
            _cam.transform.position = viewPoint.transform.position;
            _cam.transform.rotation = viewPoint.transform.rotation;
        }

    }

    void Update()
    {

        if(photonView.IsMine)
        {
            _mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;

            //ліво право
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 
                transform.rotation.eulerAngles.y + _mouseInput.x ,transform.rotation.eulerAngles.z);

            _verticalRotStore += _mouseInput.y;
            _verticalRotStore = Mathf.Clamp(_verticalRotStore, -60f, 60f);

            //верх вниз
            if(invertLook){
                viewPoint.rotation = Quaternion.Euler(_verticalRotStore, 
                    viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
            }
            else
            {
                viewPoint.rotation = Quaternion.Euler(-_verticalRotStore, 
                    viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
            }

            _moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

            if(Input.GetKey(KeyCode.LeftShift))
            {
                _activeMoveSpeed = runSpeed;
            }
            else{
                _activeMoveSpeed = moveSpeed;
            }

            float yVal = _movement.y;
            
            _movement = ((transform.forward * _moveDirection.z) + (transform.right * _moveDirection.x)).normalized * _activeMoveSpeed;

            if(!charController.isGrounded)
            {
                _movement.y = yVal;
            }

            _isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayers);

            if(Input.GetButtonDown("Jump") && _isGrounded)
            {
                _movement.y = jumpForce;
            }

            _movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;

            charController.Move(_movement * Time.deltaTime);


            if(allGuns[_selectedGun].muzzleFlash.activeInHierarchy)
            {
                _muzzleCounter -= Time.deltaTime;

                if(_muzzleCounter <= 0)
                {
                    allGuns[_selectedGun].muzzleFlash.SetActive(false);
                }
            }

            if(!_overHeated)
            {
                if(Input.GetMouseButtonDown(0))
                {
                    Shoot();

                }

                if(Input.GetMouseButton(0) && allGuns[_selectedGun].isAutomatic)
                {
                    _shotCounter -= Time.deltaTime;

                    if(_shotCounter <= 0)
                    {
                        Shoot();
                    }
                }

                _heatCouner -= coolRate * Time.deltaTime;
            }
            else
            {
                _heatCouner -= overheatCoolRate * Time.deltaTime;

                if(_heatCouner <= 0)
                {
                    _overHeated = false;

                    UIController.instance.overheatedMessage.gameObject.SetActive(false);
                }
            }

            if(_heatCouner < 0)
            {
                _heatCouner = 0f;
            }


            UIController.instance.weaponTempSlider.value = _heatCouner;


            if(Input.GetAxisRaw("Mouse ScrollWheel") > 0)
            {
                _selectedGun++;
                if(_selectedGun >= allGuns.Count)
                {
                    _selectedGun = 0;
                }
                //SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, _selectedGun);

            } 
            else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0)
            {
                _selectedGun--;
                if(_selectedGun < 0)
                {
                    _selectedGun = allGuns.Count - 1;
                }
                //SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, _selectedGun);
            }

            for(var i = 0; i < allGuns.Count; i++)
            {
                if(Input.GetKeyDown((i + 1).ToString()))
                {
                    _selectedGun = i;
                    //SwitchGun();
                    photonView.RPC("SetGun", RpcTarget.All, _selectedGun);
                }
            }

            
            anim.SetBool("grounded", _isGrounded);
            anim.SetFloat("speed", _moveDirection.magnitude);


            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if(Cursor.lockState == CursorLockMode.None)
            {
                if(Input.GetMouseButtonDown(0))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }

    private void Shoot()
    {
        Ray ray = _cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        ray.origin = _cam.transform.position;

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            if(hit.collider.gameObject.tag == "Player")
            {
                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);

                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, allGuns[_selectedGun].shotDamage, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                GameObject bulletImpactObject = Instantiate(bulletImpact, hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));

                Destroy(bulletImpactObject, 10f);
            }
        }

        _shotCounter = allGuns[_selectedGun].timeBetweenShots;


        _heatCouner += allGuns[_selectedGun].heatPerShot;

        if(_heatCouner >= maxHeat)
        {
            _heatCouner = maxHeat;

            _overHeated = true;

            UIController.instance.overheatedMessage.gameObject.SetActive(true);
        }
       

        allGuns[_selectedGun].muzzleFlash.SetActive(true);
        _muzzleCounter = muzzleDisplayTime;

    }

    [PunRPC]
    public void DealDamage(string damager, int damageAmount, int actor)
    {
        TakeDamage(damager, damageAmount, actor);
    }

    public void TakeDamage(string damager, int damageAmount, int actor)
    {
        if(photonView.IsMine)
        {

            _currentHealth -= damageAmount;

            if(_currentHealth <= 0)
            {
                _currentHealth = 0;

                PlayerSpawner.instance.Die(damager);

                MatchManager.instance.UpdateStatsSend(actor, 0, 1);
            }

            UIController.instance.healthSlider.value = _currentHealth;
        }
    }

    void SwitchGun()
    {
        foreach(Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }

        allGuns[_selectedGun].gameObject.SetActive(true);
        allGuns[_selectedGun].muzzleFlash.SetActive(false);
    }

    [PunRPC]
    public void SetGun(int gunToSwitchTo)
    {
        if(gunToSwitchTo < allGuns.Count)
        {
            _selectedGun = gunToSwitchTo;
            SwitchGun();
        }
    }

}
