using System;
using Xwt.Backends;
using Xwt.Engine;

namespace Xwt.Gdi.Backend {

    public class GdiEngine : Xwt.Backends.EngineBackend {

        public override void RunApplication() {
            RegisterBackends();
        }

        public virtual void RegisterBackends() {
            WidgetRegistry.RegisterBackend(typeof(Xwt.Drawing.Font), typeof(FontBackendHandler));

        }

        //public override void Invoke(Action action) {
        //    action();
        //}

        //public override object TimeoutInvoke(Func<bool> action, TimeSpan timeSpan) {
        //    throw new NotImplementedException();
        //}

        //public override void CancelTimeoutInvoke(object id) {
        //    throw new NotImplementedException();
        //}

        public override object GetNativeWidget(Widget w) {
            throw new NotImplementedException();
        }

        public override IWindowFrameBackend GetBackendForWindow(object nativeWindow) {
            throw new NotImplementedException();
        }

        public override void ExitApplication() {
            throw new NotImplementedException();
        }

        public override void InvokeAsync(Action action) {
            throw new NotImplementedException();
        }

        public override object TimerInvoke(Func<bool> action, TimeSpan timeSpan) {
            throw new NotImplementedException();
        }

        public override void CancelTimerInvoke(object id) {
            throw new NotImplementedException();
        }
    }
}