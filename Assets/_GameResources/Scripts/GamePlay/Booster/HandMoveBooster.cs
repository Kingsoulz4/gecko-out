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

        private List<BodyController> listBodySelected = new List<BodyController>();
        private List<Portal> listPortal = new List<Portal>();
        protected override int CurrentCount { get => UserDataManager.HandMoveBooster; set => UserDataManager.HandMoveBooster = value; }

        private void Update()
        {
            if (IsShowConfirm && Input.GetMouseButton(0))
            {
                var body = TouchInputHandler.Instance.GetBodyControllerByMouse(Input.mousePosition);
                if (body != null && listBodySelected.Contains(body))
                {
                    DoBooster(body);
                }
            }
        }

        private bool CanAddToListBody(BodyController bodyController)
        {
            bool isPortalEnabled = false;
            ColorType portalColor = ColorType.Violet;
            ColorType bodyColor = ColorType.Violet;
            foreach (var portalTmp in GameMap.Instance.ListPortal)
            {
                portalColor = portalTmp.PortalData.listColor.FirstOrDefault();
                bodyColor = bodyController.BodyData.listColor.FirstOrDefault();
                if (portalColor == bodyColor && portalTmp.IsEnablePortal
                    && !bodyController.MoveToPortal.IsEnteringPortal && !portalTmp.IsMovingToPortal)
                {
                    listPortal.Add(portalTmp);
                    isPortalEnabled = true;
                    break;
                }
            }

            return bodyController.CanControl && isPortalEnabled;
        }

        private void ShowSlectedBody()
        {
            listBodySelected.Clear();
            listPortal.Clear();
            listBodySelected = LevelManager.Instance.LevelGame.ListBody.Where(x => CanAddToListBody(x)).ToList();
            foreach (var body in listBodySelected)
            {
                if (body != null)
                    body.BodySelectedIcon.gameObject.SetActive(true);
            }
        }

        public void HideSelectedBody()
        {
            foreach (var body in listBodySelected)
            {
                if (body != null)
                    body.BodySelectedIcon.gameObject.SetActive(false);
            }
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

            HideSelectedBody();
        }

        public override void ActiveBooster()
        {
            base.ActiveBooster();
            IsShowConfirm = false;
            OnStartUseBooster?.Invoke(this, CurrentCount);
        }

        private void DoBooster(BodyController bodyController)
        {
            ActiveBooster();
            Portal portal = listPortal.Find(x => x.PortalData.listColor.FirstOrDefault() == bodyController.BodyData.listColor.FirstOrDefault());
            bodyController.MoveToPortal.EnterPortalBooster(portal);
            listBodySelected.Remove(bodyController);
        }

        protected override void ShowBooster()
        {
            base.ShowBooster();
            TouchInputHandler.Instance.CanClick = false;
            IsShowConfirm = true;
            ShowSlectedBody();
        }

        protected override void Done()
        {
            base.Done();
            TouchInputHandler.Instance.CanClick = true;
        }
    }
}
