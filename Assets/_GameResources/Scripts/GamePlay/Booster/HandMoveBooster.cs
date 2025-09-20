using DG.Tweening;
using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geckout
{
    public class HandMoveBooster : BoosterBase
    {
        protected override int CurrentCount { get => UserDataManager.HandMoveBooster; set => UserDataManager.HandMoveBooster = value; }
        private List<BodyController> listBodySelected = new List<BodyController>();

        private void Update()
        {
            if (IsShowConfirm && Input.GetMouseButton(0))
            {
                var body = TouchInputHandler.Instance.GetBodyControllerByMouse(Input.mousePosition);
                if (body != null)
                {

                }
            }
        }

        private bool CanAddToListBody(BodyController bodyController)
        {
            bool isPortalEnabled = false;
            ColorType portalColor = ColorType.Violet;
            ColorType bodyColor = ColorType.Violet;
            foreach (var portal in GameMap.Instance.ListPortal)
            {
                portalColor = portal.PortalData.listColor.FirstOrDefault();
                bodyColor = bodyController.BodyData.listColor.FirstOrDefault();
                if (portalColor == bodyColor && portal.IsEnablePortal)
                {
                    isPortalEnabled = true;
                    break;
                }
            }

            return bodyController.CanControl && isPortalEnabled;
        }

        private void InitListBody()
        {
            listBodySelected.Clear();
            listBodySelected = LevelManager.Instance.LevelGame.ListBody.Where(x => CanAddToListBody(x)).ToList();
        }

        public override void Init()
        {
            base.Init();
            CurrentCount = UserDataManager.HandMoveBooster;
        }

        public override void CancelBooster()
        {
            base.CancelBooster();
            TouchInputHandler.Instance.CanClick = true;
            IsShowConfirm = false;
        }

        public override void ActiveBooster()
        {
            base.ActiveBooster();
            IsShowConfirm = false;
            InitListBody();
            OnStartUseBooster?.Invoke(this, CurrentCount);
        }

        protected override void ShowBooster()
        {
            base.ShowBooster();
            TouchInputHandler.Instance.CanClick = false;
            IsShowConfirm = true;
        }

        protected override void Done()
        {
            base.Done();
            TouchInputHandler.Instance.CanClick = true;
        }
    }
}
