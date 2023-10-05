using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Select.Common;
using UnityEngine.InputSystem;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;
using Select.Template;

namespace Select.InputSystem
{
    /// <summary>
    /// InputSystemのオーナー
    /// </summary>
    public class InputSystemsOwner : MonoBehaviour, ISelectGameManager
    {
        /// <summary>UI用のインプットイベント</summary>
        [SerializeField] private InputUI inputUI;
        /// <summary>UI用のインプットイベント</summary>
        public InputUI InputUI => inputUI;
        /// <summary>インプットアクション</summary>
        private FutureContents3D_Select _inputActions;
        /// <summary>監視管理</summary>
        private CompositeDisposable _compositeDisposable;
        /// <summary>現在の入力モード（コントローラー／キーボード）</summary>
        private IntReactiveProperty _currentInputMode;
        /// <summary>現在の入力モード（コントローラー／キーボード）</summary>
        public IntReactiveProperty CurrentInputMode => _currentInputMode;
        /// <summary>ゲームパッド</summary>
        private Gamepad _gamepad;
        /// <summary>左モーター（低周波）の回転数</summary>
        [SerializeField] private float leftMotor = .8f;
        /// <summary>右モーター（高周波）の回転数</summary>
        [SerializeField] private float rightMotor = 0f;
        /// <summary>振動を停止するまでの時間</summary>
        [SerializeField] private float delayTime = .3f;
        /// <summary>振動を有効フラグ</summary>
        [SerializeField] private bool isVibrationEnabled;

        private void Reset()
        {
            inputUI = GetComponent<InputUI>();
        }

        public void OnStart()
        {
            _inputActions = new FutureContents3D_Select();
            _inputActions.UI.Cancel.started += inputUI.OnCanceled;
            _inputActions.UI.Cancel.performed += inputUI.OnCanceled;
            _inputActions.UI.Cancel.canceled += inputUI.OnCanceled;

            _inputActions.Enable();

            _compositeDisposable = new CompositeDisposable();
            _currentInputMode = new IntReactiveProperty((int)InputMode.Gamepad);
            // 入力モード 0:キーボード 1:コントローラー
            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                    {
                        _currentInputMode.Value = (int)InputMode.Keyboard;
                    }
                    else if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
                    {
                        _currentInputMode.Value = (int)InputMode.Gamepad;
                    }
                })
                .AddTo(_compositeDisposable);
            // ゲームパッドの情報をセット
            _gamepad = Gamepad.current;

            var tResourcesAccessory = new SelectTemplateResourcesAccessory();
            // ステージ共通設定の取得
            var mainSceneStagesConfResources = tResourcesAccessory.LoadSaveDatasCSV(ConstResorcesNames.SYSTEM_CONFIG);
            var mainSceneStagesConfs = tResourcesAccessory.GetSystemConfig(mainSceneStagesConfResources);

            isVibrationEnabled = mainSceneStagesConfs[EnumSystemConfig.VibrationEnableIndex] == 1;
        }

        public bool Exit()
        {
            try
            {
                _inputActions.Disable();
                inputUI.DisableAll();
                _compositeDisposable.Clear();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void OnDestroy()
        {
            if (!StopVibration())
                Debug.LogError("振動停止の失敗");
        }

        /// <summary>
        /// 振動の再生
        /// </summary>
        /// <returns>成功／失敗</returns>
        public bool PlayVibration()
        {
            try
            {
                if (isVibrationEnabled)
                {
                    if (_gamepad != null)
                        _gamepad.SetMotorSpeeds(leftMotor, rightMotor);
                    DOVirtual.DelayedCall(delayTime, () =>
                    {
                        if (!StopVibration())
                            Debug.LogError("振動停止の失敗");
                    });
                }
                else
                    Debug.Log("振動オフ設定済み");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        /// <summary>
        /// 振動停止
        /// </summary>
        /// <returns>成功／失敗</returns>
        private bool StopVibration()
        {
            try
            {
                _gamepad.ResetHaptics();

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }
    }

    /// <summary>
    /// 各インプットのインターフェース
    /// </summary>
    public interface IInputSystemsOwner
    {
        /// <summary>
        /// 全ての入力をリセット
        /// </summary>
        public void DisableAll();
    }

    /// <summary>
    /// 入力モード
    /// </summary>
    public enum InputMode
    {
        /// <summary>コントローラー</summary>
        Gamepad,
        /// <summary>キーボード</summary>
        Keyboard,
    }
}