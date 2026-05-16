/*===============================================================================
Copyright (C) 2024 Immersal - Part of Hexagon. All Rights Reserved.

This file is part of the Immersal SDK.

The Immersal SDK cannot be copied, distributed, or made available to
third-parties for commercial purposes without written permission of Immersal Ltd.

Contact sales@immersal.com for licensing requests.
===============================================================================*/

using System.Collections.Generic;
using UnityEngine;

namespace Immersal.Samples.Navigation
{
    public class IsNavigationTarget : MonoBehaviour
    {
        public NavigationTargets.NavigationCategory navigationCategory = NavigationTargets.NavigationCategory.Locations;
        public string targetName;
        public Sprite icon;
        public Vector3 position
        {
            get
            {
                return m_collider.bounds.center;
            }

            set
            {

            }
        }

        private Collider m_collider = null;
        private static HapticNavigationFeedback s_hapticFeedback = null;

        private void Start()
        {
            NavigationGraphManager.Instance?.AddTarget(this);

            // Find haptic feedback manager once at the start
            if (s_hapticFeedback == null)
            {
                s_hapticFeedback = FindObjectOfType<HapticNavigationFeedback>();
                
                if (s_hapticFeedback == null)
                {
                    Debug.LogWarning("HapticNavigationFeedback not found in scene. Haptic feedback will not work.");
                }
            }
        }

        private void OnDestroy()
        {
            NavigationGraphManager.Instance?.RemoveTarget(this);
        }

        private void OnEnable()
        {
            m_collider = GetComponent<Collider>();

            if (!NavigationTargets.NavigationTargetsDict.ContainsKey(navigationCategory))
                NavigationTargets.NavigationTargetsDict[navigationCategory] = new List<GameObject>();

            NavigationTargets.NavigationTargetsDict[navigationCategory].Add(gameObject);

            if (targetName.Equals(""))
            {
                targetName = gameObject.name;
            }
        }

        private void OnDisable()
        {
            if (NavigationTargets.NavigationTargetsDict.ContainsKey(navigationCategory))
                NavigationTargets.NavigationTargetsDict[navigationCategory].Remove(gameObject);
        }

        public void OnTargetSelected()
        {
            if (s_hapticFeedback != null)
            {
                s_hapticFeedback.StartNavigation();
                Debug.Log($"Started haptic navigation to: {targetName}");
            }
        }
        
        public void OnTargetDeselected()
        {
            if (s_hapticFeedback != null)
            {
                s_hapticFeedback.StopNavigation();
                Debug.Log($"Stopped haptic navigation to: {targetName}");
            }
        }
    }
}