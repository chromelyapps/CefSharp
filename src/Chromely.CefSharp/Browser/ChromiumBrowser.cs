// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using System;
using System.Threading;
using CefSharp;
using CefSharp.Internals;
using Chromely.Core.Configuration;
using Chromely.Core.Host;
using Chromely.Core.Logging;
using Microsoft.Extensions.Logging;
using static Chromely.Interop.User32;

using CefSharpBrowserSettings = CefSharp.BrowserSettings;

namespace Chromely.CefSharp.Browser
{
    /// <summary>
    /// ChromiumBrowser is the WinForms web browser control
    /// </summary>
    public abstract partial class ChromiumBrowser : IWebBrowserInternal
    {
        protected IChromelyConfiguration _config;
        protected IntPtr _browserWindowHandle;

        /// <summary>
        /// The settings.
        /// </summary>
        protected CefSettings _settings;

        /// <summary>
        /// The managed cef browser adapter
        /// </summary>
        protected IBrowserAdapter _managedCefBrowserAdapter;
        /// <summary>
        /// The browser
        /// </summary>
        protected IBrowser _browser;
        /// <summary>
        /// A flag that indicates whether or not <see cref="InitializeFieldsAndCefIfRequired"/> has been called.
        /// </summary>
        protected bool _initialized;
        /// <summary>
        /// Has the underlying Cef Browser been created (slightly different to initliazed in that
        /// the browser is initialized in an async fashion)
        /// </summary>
        protected bool _browserCreated;
        /// <summary>
        /// A flag indicating if the <see cref="Address"/> was used when calling CreateBrowser
        /// If false and <see cref="Address"/> contains a non empty string Load will be called
        /// on the main frame
        /// </summary>
        protected bool _initialAddressLoaded;
        /// <summary>
        /// If true the the WS_EX_NOACTIVATE style will be removed so that future mouse clicks
        /// inside the browser correctly activate and focus the window.
        /// </summary>
        protected bool _removeExNoActivateStyle;
        /// <summary>
        /// Browser initialization settings
        /// </summary>
        protected IBrowserSettings _browserSettings;
        /// <summary>
        /// The request context (we deliberately use a private variable so we can throw an exception if
        /// user attempts to set after browser created)
        /// </summary>
        protected IRequestContext _requestContext;

        /// <summary>
        /// The value for disposal, if it's 1 (one) then this instance is either disposed
        /// or in the process of getting disposed
        /// </summary>
        protected int _disposeSignaled;

        /// <summary>
        /// Used as workaround for issue https://github.com/cefsharp/CefSharp/issues/3021
        /// </summary>
        protected long _canExecuteJavascriptInMainFrameId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChromiumBrowser"/> class.
        /// </summary>
        public ChromiumBrowser(IChromelyNativeHost nativeHost, IChromelyConfiguration config)
        {
            NativeHost = nativeHost;
            _config = config;
        }

        public IChromelyNativeHost NativeHost { get; private set; }

        public void Initialize(CefSettings settings)
        {
            _settings = settings;
            InitializeFieldsAndCefIfRequired();
        }

        /// <summary>
        /// Set to true while handing an activating WM_ACTIVATE message.
        /// MUST ONLY be cleared by DefaultFocusHandler.
        /// </summary>
        /// <value><c>true</c> if this instance is activating; otherwise, <c>false</c>.</value>
        public bool IsActivating { get; set; }

        /// <summary>
        /// Gets or sets the browser settings.
        /// </summary>
        /// <value>The browser settings.</value>
        public IBrowserSettings BrowserSettings
        {
            get
            {
                //We keep a reference to the browserSettings for the case where
                //the Control Handle is destroyed then Created see https://github.com/cefsharp/CefSharp/issues/2840
                //As it's not possible to change settings after the browser has been
                //created, and changing browserSettings then creating a new handle will
                //give a subtle different user experience if you aren't expecting it we
                //return null here even though we still have a reference.
                if (_browserCreated)
                {
                    return null;
                }
                return _browserSettings;
            }
            set
            {
                if (_browserCreated)
                {
                    throw new Exception("Browser has already been created. BrowserSettings must be " +
                                        "set before the underlying CEF browser is created.");
                }
                if (value != null && value.GetType() != typeof(BrowserSettings))
                {
                    throw new Exception(string.Format("BrowserSettings can only be of type {0} or null", typeof(BrowserSettings)));
                }
                _browserSettings = value;
            }
        }
        /// <summary>
        /// Activates browser upon creation, the default value is false. Prior to version 73
        /// the default behaviour was to activate browser on creation (Equivilent of setting this property to true).
        /// To restore this behaviour set this value to true immediately after you create the <see cref="ChromiumBrowser"/> instance.
        /// https://bitbucket.org/chromiumembedded/cef/issues/1856/branch-2526-cef-activates-browser-window
        /// </summary>
        public bool ActivateBrowserOnCreation { get; set; }
        /// <summary>
        /// Gets or sets the request context.
        /// </summary>
        /// <value>The request context.</value>
        public IRequestContext RequestContext
        {
            get { return _requestContext; }
            set
            {
                if (_browserCreated)
                {
                    throw new Exception("Browser has already been created. RequestContext must be " +
                                        "set before the underlying CEF browser is created.");
                }
                if (value != null && value.GetType() != typeof(RequestContext))
                {
                    throw new Exception(string.Format("RequestContext can only be of type {0} or null", typeof(RequestContext)));
                }
                _requestContext = value;
            }
        }
        /// <summary>
        /// A flag that indicates whether the control is currently loading one or more web pages (true) or not (false).
        /// </summary>
        /// <value><c>true</c> if this instance is loading; otherwise, <c>false</c>.</value>
        /// <remarks>In the WPF control, this property is implemented as a Dependency Property and fully supports data
        /// binding.</remarks>
        public bool IsLoading { get; private set; }
        /// <summary>
        /// The text that will be displayed as a ToolTip
        /// </summary>
        /// <value>The tooltip text.</value>
        public string TooltipText { get; private set; }
        /// <summary>
        /// The address (URL) which the browser control is currently displaying.
        /// Will automatically be updated as the user navigates to another page (e.g. by clicking on a link).
        /// </summary>
        /// <value>The address.</value>
        /// <remarks>In the WPF control, this property is implemented as a Dependency Property and fully supports data
        /// binding.</remarks>
        public string Address { get; private set; }

        /// <summary>
        /// Implement <see cref="IDialogHandler" /> and assign to handle dialog events.
        /// </summary>
        /// <value>The dialog handler.</value>
        public IDialogHandler DialogHandler { get; set; }
        /// <summary>
        /// Implement <see cref="IJsDialogHandler" /> and assign to handle events related to JavaScript Dialogs.
        /// </summary>
        /// <value>The js dialog handler.</value>
        public IJsDialogHandler JsDialogHandler { get; set; }
        /// <summary>
        /// Implement <see cref="IKeyboardHandler" /> and assign to handle events related to key press.
        /// </summary>
        /// <value>The keyboard handler.</value>
        public IKeyboardHandler KeyboardHandler { get; set; }
        /// <summary>
        /// Implement <see cref="IRequestHandler" /> and assign to handle events related to browser requests.
        /// </summary>
        /// <value>The request handler.</value>
        public IRequestHandler RequestHandler { get; set; }
        /// <summary>
        /// Implement <see cref="IDownloadHandler" /> and assign to handle events related to downloading files.
        /// </summary>
        /// <value>The download handler.</value>
        public IDownloadHandler DownloadHandler { get; set; }
        /// <summary>
        /// Implement <see cref="ILoadHandler" /> and assign to handle events related to browser load status.
        /// </summary>
        /// <value>The load handler.</value>
        public ILoadHandler LoadHandler { get; set; }
        /// <summary>
        /// Implement <see cref="ILifeSpanHandler" /> and assign to handle events related to popups.
        /// </summary>
        /// <value>The life span handler.</value>
        public ILifeSpanHandler LifeSpanHandler { get; set; }
        /// <summary>
        /// Implement <see cref="IDisplayHandler" /> and assign to handle events related to browser display state.
        /// </summary>
        /// <value>The display handler.</value>
        public IDisplayHandler DisplayHandler { get; set; }
        /// <summary>
        /// Implement <see cref="IContextMenuHandler" /> and assign to handle events related to the browser context menu
        /// </summary>
        /// <value>The menu handler.</value>
        public IContextMenuHandler MenuHandler { get; set; }
        /// <summary>
        /// Implement <see cref="IRenderProcessMessageHandler" /> and assign to handle messages from the render process.
        /// </summary>
        /// <value>The render process message handler.</value>
        public IRenderProcessMessageHandler RenderProcessMessageHandler { get; set; }
        /// <summary>
        /// Implement <see cref="IFindHandler" /> to handle events related to find results.
        /// </summary>
        /// <value>The find handler.</value>
        public IFindHandler FindHandler { get; set; }
        /// <summary>
        /// The <see cref="IFocusHandler" /> for this ChromiumBrowser.
        /// </summary>
        /// <value>The focus handler.</value>
        /// <remarks>If you need customized focus handling behavior for WinForms, the suggested
        /// best practice would be to inherit from DefaultFocusHandler and try to avoid
        /// needing to override the logic in OnGotFocus. The implementation in
        /// DefaultFocusHandler relies on very detailed behavior of how WinForms and
        /// Windows interact during window activation.</remarks>
        public IFocusHandler FocusHandler { get; set; }
        /// <summary>
        /// Implement <see cref="IDragHandler" /> and assign to handle events related to dragging.
        /// </summary>
        /// <value>The drag handler.</value>
        public IDragHandler DragHandler { get; set; }
        /// <summary>
        /// Implement <see cref="IResourceRequestHandlerFactory" /> and control the loading of resources
        /// </summary>
        /// <value>The resource handler factory.</value>
        public IResourceRequestHandlerFactory ResourceRequestHandlerFactory { get; set; }

        /// <summary>
        /// Event handler that will get called when the resource load for a navigation fails or is canceled.
        /// It's important to note this event is fired on a CEF UI thread, which by default is not the same as your application UI
        /// thread. It is unwise to block on this thread for any length of time as your browser will become unresponsive and/or hang..
        /// To access UI elements you'll need to Invoke/Dispatch onto the UI Thread.
        /// </summary>
        public event EventHandler<LoadErrorEventArgs> LoadError;
        /// <summary>
        /// Event handler that will get called when the browser begins loading a frame. Multiple frames may be loading at the same
        /// time. Sub-frames may start or continue loading after the main frame load has ended. This method may not be called for a
        /// particular frame if the load request for that frame fails. For notification of overall browser load status use
        /// OnLoadingStateChange instead.
        /// It's important to note this event is fired on a CEF UI thread, which by default is not the same as your application UI
        /// thread. It is unwise to block on this thread for any length of time as your browser will become unresponsive and/or hang..
        /// To access UI elements you'll need to Invoke/Dispatch onto the UI Thread.
        /// </summary>
        /// <remarks>Whilst this may seem like a logical place to execute js, it's called before the DOM has been loaded, implement
        /// <see cref="IRenderProcessMessageHandler.OnContextCreated" /> as it's called when the underlying V8Context is created
        /// </remarks>
        public event EventHandler<FrameLoadStartEventArgs> FrameLoadStart;
        /// <summary>
        /// Event handler that will get called when the browser is done loading a frame. Multiple frames may be loading at the same
        /// time. Sub-frames may start or continue loading after the main frame load has ended. This method will always be called
        /// for all frames irrespective of whether the request completes successfully.
        /// It's important to note this event is fired on a CEF UI thread, which by default is not the same as your application UI
        /// thread. It is unwise to block on this thread for any length of time as your browser will become unresponsive and/or hang..
        /// To access UI elements you'll need to Invoke/Dispatch onto the UI Thread.
        /// </summary>
        public event EventHandler<FrameLoadEndEventArgs> FrameLoadEnd;
        /// <summary>
        /// Event handler that will get called when the Loading state has changed.
        /// This event will be fired twice. Once when loading is initiated either programmatically or
        /// by user action, and once when loading is terminated due to completion, cancellation of failure.
        /// It's important to note this event is fired on a CEF UI thread, which by default is not the same as your application UI
        /// thread. It is unwise to block on this thread for any length of time as your browser will become unresponsive and/or hang..
        /// To access UI elements you'll need to Invoke/Dispatch onto the UI Thread.
        /// </summary>
        public event EventHandler<LoadingStateChangedEventArgs> LoadingStateChanged;
        /// <summary>
        /// Event handler for receiving Javascript console messages being sent from web pages.
        /// It's important to note this event is fired on a CEF UI thread, which by default is not the same as your application UI
        /// thread. It is unwise to block on this thread for any length of time as your browser will become unresponsive and/or hang..
        /// To access UI elements you'll need to Invoke/Dispatch onto the UI Thread.
        /// (The exception to this is when your running with settings.MultiThreadedMessageLoop = false, then they'll be the same thread).
        /// </summary>
        public event EventHandler<ConsoleMessageEventArgs> ConsoleMessage;
        /// <summary>
        /// Event handler for changes to the status message.
        /// It's important to note this event is fired on a CEF UI thread, which by default is not the same as your application UI
        /// thread. It is unwise to block on this thread for any length of time as your browser will become unresponsive and/or hang.
        /// To access UI elements you'll need to Invoke/Dispatch onto the UI Thread.
        /// (The exception to this is when your running with settings.MultiThreadedMessageLoop = false, then they'll be the same thread).
        /// </summary>
        public event EventHandler<StatusMessageEventArgs> StatusMessage;
        /// <summary>
        /// Occurs when the browser address changed.
        /// It's important to note this event is fired on a CEF UI thread, which by default is not the same as your application UI
        /// thread. It is unwise to block on this thread for any length of time as your browser will become unresponsive and/or hang..
        /// To access UI elements you'll need to Invoke/Dispatch onto the UI Thread.
        /// </summary>
        public event EventHandler<AddressChangedEventArgs> AddressChanged;
        /// <summary>
        /// Occurs when the browser title changed.
        /// It's important to note this event is fired on a CEF UI thread, which by default is not the same as your application UI
        /// thread. It is unwise to block on this thread for any length of time as your browser will become unresponsive and/or hang..
        /// To access UI elements you'll need to Invoke/Dispatch onto the UI Thread.
        /// </summary>
        public event EventHandler<TitleChangedEventArgs> TitleChanged;
        /// <summary>
        /// Event called after the underlying CEF browser instance has been created. 
        /// It's important to note this event is fired on a CEF UI thread, which by default is not the same as your application UI
        /// thread. It is unwise to block on this thread for any length of time as your browser will become unresponsive and/or hang..
        /// To access UI elements you'll need to Invoke/Dispatch onto the UI Thread.
        /// </summary>
        public event EventHandler IsBrowserInitializedChanged;

        /// <summary>
        /// Event handler that will get called when the message that originates from CefSharp.PostMessage
        /// </summary>
        public event EventHandler<JavascriptMessageReceivedEventArgs> JavascriptMessageReceived;

        /// <summary>
        /// A flag that indicates whether the state of the control currently supports the GoForward action (true) or not (false).
        /// </summary>
        /// <value><c>true</c> if this instance can go forward; otherwise, <c>false</c>.</value>
        /// <remarks>In the WPF control, this property is implemented as a Dependency Property and fully supports data
        /// binding.</remarks>
        public bool CanGoForward { get; private set; }
        /// <summary>
        /// A flag that indicates whether the state of the control current supports the GoBack action (true) or not (false).
        /// </summary>
        /// <value><c>true</c> if this instance can go back; otherwise, <c>false</c>.</value>
        /// <remarks>In the WPF control, this property is implemented as a Dependency Property and fully supports data
        /// binding.</remarks>
        public bool CanGoBack { get; private set; }
        /// <summary>
        /// A flag that indicates whether the WebBrowser is initialized (true) or not (false).
        /// </summary>
        /// <value><c>true</c> if this instance is browser initialized; otherwise, <c>false</c>.</value>
        public bool IsBrowserInitialized { get; private set; }

        /// <summary>
        /// A flag that indicates if you can execute javascript in the main frame.
        /// Flag is set to true in IRenderProcessMessageHandler.OnContextCreated.
        /// and false in IRenderProcessMessageHandler.OnContextReleased
        /// </summary>
        public bool CanExecuteJavascriptInMainFrame { get; private set; }

        /// <summary>
        /// Handles the <see cref="E:ApplicationExit" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void OnApplicationExit(object sender, EventArgs e)
        {
            Cef.Shutdown();
        }

        /// <summary>
        /// Required for designer support - this method cannot be inlined as the designer
        /// will attempt to load libcef.dll and will subsiquently throw an exception.
        /// TODO: Still not happy with this method name, need something better
        /// </summary>
        private void InitializeFieldsAndCefIfRequired()
        {
            if (!_initialized)
            {
                if (!Cef.IsInitialized && !Cef.Initialize(_settings))
                {
                    throw new InvalidOperationException("Cef::Initialize() failed");
                }

                Cef.AddDisposable(this);


                if (ResourceRequestHandlerFactory == null)
                {
                    ResourceRequestHandlerFactory = new ResourceRequestHandlerFactory();
                }

                if (_browserSettings == null)
                {
                    _browserSettings = CefSharpBrowserSettings.Create(true);
                }

                _managedCefBrowserAdapter = ManagedCefBrowserAdapter.Create(this, false);

                _initialized = true;
            }
        }

        /// <summary>
        /// Loads the specified URL.
        /// </summary>
        /// <param name="url">The URL to be loaded.</param>
        void IWebBrowser.Load(string url)
        {
            if (IsBrowserInitialized)
            {
                using (var frame = this.GetMainFrame())
                {
                    frame.LoadUrl(url);
                }
            }
            else
            {
                Address = url;
            }
        }

        /// <summary>
        /// The javascript object repository, one repository per ChromiumBrowser instance.
        /// </summary>
        public IJavascriptObjectRepository JavascriptObjectRepository
        {
            get
            {
                InitializeFieldsAndCefIfRequired();
                return _managedCefBrowserAdapter == null ? null : _managedCefBrowserAdapter.JavascriptObjectRepository;
            }
        }

        /// <summary>
        /// Override this method to handle creation of WindowInfo. This method can be used to customise aspects of
        /// browser creation including configuration of settings such as <see cref="IWindowInfo.ExStyle"/>.
        /// Window Activation is disabled by default, you can re-enable it by overriding and removing the
        /// WS_EX_NOACTIVATE style from <see cref="IWindowInfo.ExStyle"/>.
        /// </summary>
        /// <param name="handle">Window handle for the Control</param>
        /// <returns>Window Info</returns>
        /// <example>
        /// To re-enable Window Activation then remove WS_EX_NOACTIVATE from ExStyle
        /// <code>
        /// const uint WS_EX_NOACTIVATE = 0x08000000;
        /// windowInfo.ExStyle &amp;= ~WS_EX_NOACTIVATE;
        ///</code>
        /// </example>
        internal IWindowInfo CreateBrowserWindowInfo(IntPtr handle)
        {
            var windowInfo = new WindowInfo();
            windowInfo.SetAsChild(handle);

            if (!ActivateBrowserOnCreation)
            {
                //Disable Window activation by default
                //https://bitbucket.org/chromiumembedded/cef/issues/1856/branch-2526-cef-activates-browser-window
                windowInfo.ExStyle |= (uint)WS_EX.NOACTIVATE;
            }

            return windowInfo;
        }

        internal void NotifyMoveOrResize()
        {
            if (IsBrowserInitialized)
            {
                _browser?.GetHost()?.NotifyMoveOrResizeStarted();
            }
        }

        internal void CreateBrowser(IntPtr parent)
        {
            if (((IWebBrowserInternal)this).HasParent == false)
            {
                if (IsBrowserInitialized == false || _browser == null)
                {
                    _requestContext = Cef.GetGlobalRequestContext();

                    var windowInfo = CreateBrowserWindowInfo(parent);

                    //We actually check if WS_EX_NOACTIVATE was set for instances
                    //the user has override CreateBrowserWindowInfo and not called base.CreateBrowserWindowInfo
                    _removeExNoActivateStyle = ((WS_EX)windowInfo.ExStyle & WS_EX.NOACTIVATE) == WS_EX.NOACTIVATE;

                    _initialAddressLoaded = !string.IsNullOrEmpty(_config?.StartUrl);
                    Address = _config?.StartUrl;

                    _browserSettings.DefaultEncoding = "UTF-8";
                    _browserSettings.FileAccessFromFileUrls = CefState.Enabled;
                    _browserSettings.UniversalAccessFromFileUrls = CefState.Enabled;
                    _browserSettings.WebSecurity = CefState.Disabled;

                    _managedCefBrowserAdapter.CreateBrowser(windowInfo, _browserSettings, _requestContext, Address);
                }
                else
                {
                    // If the browser already exists we'll reparent it to the new Handle
                    var browserHandle = _browser.GetHost().GetWindowHandle();
                    NativeMethodWrapper.SetWindowParent(browserHandle, parent);
                }

                Logger.Instance.Log.LogInformation("Cef browser successfully created.");
            }
        }

        /// <summary>
        /// Called after browser created.
        /// </summary>
        /// <param name="browser">The browser.</param>
        void IWebBrowserInternal.OnAfterBrowserCreated(IBrowser browser)
        {
            _browser = browser;
            IsBrowserInitialized = true;

            // If Load was called after the call to CreateBrowser we'll call Load
            // on the MainFrame
            if (!_initialAddressLoaded && !string.IsNullOrEmpty(Address))
            {
                browser.MainFrame.LoadUrl(Address);
            }

            var host = this.GetBrowserHost();
            _browserWindowHandle = host.GetWindowHandle();

            // Set Javascript execution
            this._config.JavaScriptExecutor = new DefaultJavaScriptExecutor(_browser);

            IsBrowserInitializedChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets the address.
        /// </summary>
        /// <param name="args">The <see cref="AddressChangedEventArgs"/> instance containing the event data.</param>
        void IWebBrowserInternal.SetAddress(AddressChangedEventArgs args)
        {
            Address = args.Address;

            var handler = AddressChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// Sets the loading state change.
        /// </summary>
        /// <param name="args">The <see cref="LoadingStateChangedEventArgs"/> instance containing the event data.</param>
        void IWebBrowserInternal.SetLoadingStateChange(LoadingStateChangedEventArgs args)
        {
            CanGoBack = args.CanGoBack;
            CanGoForward = args.CanGoForward;
            IsLoading = args.IsLoading;

            if (_removeExNoActivateStyle && _browser != null)
            {
                _removeExNoActivateStyle = false;

                var host = this.GetBrowserHost();
                var hwnd = host.GetWindowHandle();
                //Remove the WS_EX_NOACTIVATE style so that future mouse clicks inside the
                //browser correctly activate and focus the browser. 
                //https://github.com/chromiumembedded/cef/blob/9df4a54308a88fd80c5774d91c62da35afb5fd1b/tests/cefclient/browser/root_window_win.cc#L1088
                NativeMethodWrapper.RemoveExNoActivateStyle(hwnd);
            }

            var handler = LoadingStateChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// Sets the title.
        /// </summary>
        /// <param name="args">The <see cref="TitleChangedEventArgs"/> instance containing the event data.</param>
        void IWebBrowserInternal.SetTitle(TitleChangedEventArgs args)
        {
            var handler = TitleChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// Sets the tooltip text.
        /// </summary>
        /// <param name="tooltipText">The tooltip text.</param>
        void IWebBrowserInternal.SetTooltipText(string tooltipText)
        {
            TooltipText = tooltipText;
        }

        /// <summary>
        /// Handles the <see cref="E:FrameLoadStart" /> event.
        /// </summary>
        /// <param name="args">The <see cref="FrameLoadStartEventArgs"/> instance containing the event data.</param>
        void IWebBrowserInternal.OnFrameLoadStart(FrameLoadStartEventArgs args)
        {
            var handler = FrameLoadStart;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// Handles the <see cref="E:FrameLoadEnd" /> event.
        /// </summary>
        /// <param name="args">The <see cref="FrameLoadEndEventArgs"/> instance containing the event data.</param>
        void IWebBrowserInternal.OnFrameLoadEnd(FrameLoadEndEventArgs args)
        {
            var handler = FrameLoadEnd;
            if (handler != null)
            {
                handler(this, args);
            }

            // Setup window subclass to intercept message for frameless window dragging
            NativeHost.SetupMessageInterceptor(_browserWindowHandle);
        }

        /// <summary>
        /// Handles the <see cref="E:ConsoleMessage" /> event.
        /// </summary>
        /// <param name="args">The <see cref="ConsoleMessageEventArgs"/> instance containing the event data.</param>
        void IWebBrowserInternal.OnConsoleMessage(ConsoleMessageEventArgs args)
        {
            var handler = ConsoleMessage;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// Handles the <see cref="E:StatusMessage" /> event.
        /// </summary>
        /// <param name="args">The <see cref="StatusMessageEventArgs"/> instance containing the event data.</param>
        void IWebBrowserInternal.OnStatusMessage(StatusMessageEventArgs args)
        {
            var handler = StatusMessage;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// Handles the <see cref="E:LoadError" /> event.
        /// </summary>
        /// <param name="args">The <see cref="LoadErrorEventArgs"/> instance containing the event data.</param>
        void IWebBrowserInternal.OnLoadError(LoadErrorEventArgs args)
        {
            var handler = LoadError;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        void IWebBrowserInternal.SetCanExecuteJavascriptOnMainFrame(long frameId, bool canExecute)
        {
            //When loading pages of a different origin the frameId changes
            //For the first loading of a new origin the messages from the render process
            //Arrive in a different order than expected, the OnContextCreated message
            //arrives before the OnContextReleased, then the message for OnContextReleased
            //incorrectly overrides the value
            //https://github.com/cefsharp/CefSharp/issues/3021

            if (frameId > _canExecuteJavascriptInMainFrameId && !canExecute)
            {
                return;
            }

            _canExecuteJavascriptInMainFrameId = frameId;
            CanExecuteJavascriptInMainFrame = canExecute;
        }

        void IWebBrowserInternal.SetJavascriptMessageReceived(JavascriptMessageReceivedEventArgs args)
        {
            JavascriptMessageReceived?.Invoke(this, args);
        }

        /// <summary>
        /// Gets the browser adapter.
        /// </summary>
        /// <value>The browser adapter.</value>
        IBrowserAdapter IWebBrowserInternal.BrowserAdapter
        {
            get { return _managedCefBrowserAdapter; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has parent.
        /// </summary>
        /// <value><c>true</c> if this instance has parent; otherwise, <c>false</c>.</value>
        bool IWebBrowserInternal.HasParent { get; set; }

        public bool IsDisposed
        {
            get
            {
                return Interlocked.CompareExchange(ref _disposeSignaled, 1, 1) == 1;
            }
        }

        public IAudioHandler AudioHandler { get; set; }

        /// <summary>
        /// Returns the current IBrowser Instance
        /// </summary>
        /// <returns>browser instance or null</returns>
        IBrowser IWebBrowser.GetBrowser()
        {
            return _browser;
        }

        bool IWebBrowser.Focus()
        {
            return true;
        }
    }
}
