﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

namespace Seismograph
{
    public static class PageExtensions
    {

        // Desired width for the settings UI. UI guidelines specify this should be 346 or 646 depending on your needs.
        private const double SettingsWidth = 346;

        private static List<Popup> popupStack;
        private static Popup currentPopup;

        public static void CloseCurrentFlyout(this Page page)
        {
            if (currentPopup != null)
            {
                Storyboard sb = new Storyboard();
                
                // Normally we could just use PageThemeTransition, but that trips up a bug in WinRT that causes
                // the app to crash.  So we do our own Storyboard instead so we can find out when the animation
                // is completed.
                Rect windowBounds = Window.Current.Bounds;
                DoubleAnimation animation = new DoubleAnimation()
                {
                    From = Canvas.GetLeft(currentPopup),
                    To = windowBounds.Right,
                    Duration = new Duration(TimeSpan.FromMilliseconds(200))
                };                

                Storyboard storyboard = new Storyboard();
                storyboard.FillBehavior = FillBehavior.HoldEnd;
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, currentPopup);
                Storyboard.SetTargetProperty(animation, "(Canvas.Left)");
                storyboard.Begin();
                storyboard.Completed += (s, e) =>
                {
                    if (currentPopup != null)
                    {
                        currentPopup.IsOpen = false;
                    }
                };
            }
        }

        /// <summary>
        /// Popup the given page as a flyout from the right side of the window.
        /// </summary>
        /// <param name="page">The page to pop up</param>
        /// <param name="action">The action to invoke when the flyout is closed</param>
        public static void Flyout(this Page page, string name, Action action)
        {
            Rect windowBounds = Window.Current.Bounds;

            double desiredWidth = SettingsWidth;

            // might be docked...
            if (desiredWidth > windowBounds.Width)
            {
                desiredWidth = windowBounds.Width;
            }

            // Create a Popup window which will contain our flyout.
            var popup = new Popup();
            popup.Closed += OnPopupClosed;
            Window.Current.Activated += OnWindowActivated;
            popup.IsLightDismissEnabled = true;
            popup.Width = desiredWidth;
            popup.Height = windowBounds.Height;

            // Add the proper animation for the panel.            
            //currentPopup.ChildTransitions = new TransitionCollection();
            //currentPopup.ChildTransitions.Add(new PaneThemeTransition()
            //{
            //    Edge = EdgeTransitionLocation.Right
            //});            

            // Create a SettingsFlyout the same dimenssions as the Popup.
            page.Width = desiredWidth;
            page.Height = windowBounds.Height;

            // Place the SettingsFlyout inside our Popup window.
            popup.Child = page;

            // Let's define the location of our Popup.
            popup.SetValue(Canvas.LeftProperty, windowBounds.Width - desiredWidth);
            popup.SetValue(Canvas.TopProperty, 0);
            popup.Tag = action;

            OpenPopup(popup, name);
        }

        private static void OpenPopup(Popup popup, string name)
        {
            if (popupStack == null)
            {
                popupStack = new List<Popup>();
            }
            if (currentPopup != null)
            {
                popupStack.Add(currentPopup);
            }
            currentPopup = popup;
            currentPopup.IsOpen = true;
        }


        /// <summary>
        /// We use the window's activated event to force closing the Popup since a user maybe interacted with
        /// something that didn't normally trigger an obvious dismiss.
        /// </summary>
        /// <param name="sender">Instance that triggered the event.</param>
        /// <param name="e">Event data describing the conditions that led to the event.</param>
        static void OnWindowActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                CloseAllPopups();
            }
        }

        private static void CloseAllPopups()
        {
            if (popupStack != null && popupStack.Count > 0)
            {
                foreach (Popup p in popupStack)
                {
                    p.IsOpen = false;
                }
                popupStack.Clear();
            }
            if (currentPopup != null)
            {
                currentPopup.IsOpen = false;
            }
            
        }

        /// <summary>
        /// When the Popup closes we no longer need to monitor activation changes.
        /// </summary>
        /// <param name="sender">Instance that triggered the event.</param>
        /// <param name="e">Event data describing the conditions that led to the event.</param>
        static void OnPopupClosed(object sender, object e)
        {
            // The closed event happens before the animation is finished.  To be safe
            // we have to wait for the animation to complete.
            Popup popup = (Popup)sender;

            Action closeAction = popup.Tag as Action;
            if (closeAction != null)
            {
                closeAction();
            }

            if (popupStack == null || popupStack.Count == 0)
            {
                Window.Current.Activated -= OnWindowActivated;
                currentPopup = null;
            }
            else
            {
                int last = popupStack.Count - 1;
                popup = popupStack[last];
                Page page = popup.Child as Page;
                popupStack.RemoveAt(last);
                currentPopup = null;
                OpenPopup(popup, "OpenPopupClosed pushing new stack");
            }

        }

    }
}
