﻿using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace BowArrow
{
    public class Arrow : XRGrabInteractable
    {
        [SerializeField] private float speed = 2000.0f;
        [SerializeField] private Transform tip;
        [SerializeField] private LayerMask layerMask;

        private bool _isFlying;
        private Vector3 _lastPosition = Vector3.zero;
        private Rigidbody _rigidbody;
        private RaycastHit _hit;

        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody>();
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            if (args.interactorObject is not Notch notch) return;
            if (notch.CanRelease) LaunchArrow(notch);
        }

        private void LaunchArrow(Notch notch)
        {
            _isFlying = true;
            ApplyForce(notch.PullMeasurer);
            StartCoroutine(LaunchRoutine());
        }

        private void ApplyForce(PullMeasurer pullMeasurer)
        {
            _rigidbody.AddForce(transform.forward * (pullMeasurer.PullAmount * speed));
        }

        private IEnumerator LaunchRoutine()
        {
            // Set direction while flying
            while (!CheckForCollision(out _hit))
            {
                SetDirection();
                yield return null;
            }

            // Once the arrow has stopped flying
            DisablePhysics();
            ChildArrow(_hit);
            CheckForHittable(_hit);
        }

        private bool CheckForCollision(out RaycastHit hit)
        {
            if (_lastPosition == Vector3.zero)
                _lastPosition = tip.position;

            bool collided = Physics.Linecast(_lastPosition, tip.position, out hit, layerMask);
            _lastPosition = collided ? _lastPosition : tip.position;

            return collided;
        }

        private void SetDirection()
        {
            if (_rigidbody.velocity.z > 0.5f)
                transform.forward = _rigidbody.velocity;
        }

        private void DisablePhysics()
        {
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
        }

        private void ChildArrow(RaycastHit hit)
        {
            transform.SetParent(hit.transform);
        }

        private void CheckForHittable(RaycastHit hit)
        {
            if (hit.transform.TryGetComponent(out Target hittable))
                hittable.Hit(this);
        }

        public override bool IsSelectableBy(IXRSelectInteractor interactor)
        {
            return base.IsSelectableBy(interactor) && !_isFlying;
        }
    }
}