using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.Warmup.Models;
using Orchard.UI.Notify;
using Orchard.Warmup.Services;
using Orchard.Warmup.ViewModels;

namespace Orchard.Warmup.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly ISiteService _siteService;
        private readonly IWarmupReportManager _reportManager;
        private readonly IWarmupScheduler _warmupScheduler;

        public AdminController(
            IOrchardServices services,
            ISiteService siteService,
            IWarmupReportManager reportManager,
            IWarmupScheduler warmupScheduler) {
            _siteService = siteService;
            _reportManager = reportManager;
            Services = services;
            _warmupScheduler = warmupScheduler;

            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var warmupPart = Services.WorkContext.CurrentSite.As<WarmupSettingsPart>();

            var viewModel = new WarmupViewModel {
                Settings = warmupPart
            };

            return View(viewModel);
        }

        public ActionResult Status(PagerParameters pagerParameters) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to view warmup status")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var pagerShape = Services.New.Pager(pager).TotalItemCount(_reportManager.GetReportCount());
            var warmupPart = Services.WorkContext.CurrentSite.As<WarmupSettingsPart>();

            var model = new WarmupViewModel {
                Settings = warmupPart,
                ReportEntries = _reportManager.Read(pager.GetStartIndex(), pager.PageSize),
                Pager = pagerShape
            };
            return View(model);
        }

        [FormValueRequired("submit")]
        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var viewModel = new WarmupViewModel {
                Settings = Services.WorkContext.CurrentSite.As<WarmupSettingsPart>(),
                ReportEntries = Enumerable.Empty<ReportEntry>()
            };

            if (TryUpdateModel(viewModel)) {
                if (!String.IsNullOrEmpty(viewModel.Settings.Urls)) {
                    using (var urlReader = new StringReader(viewModel.Settings.Urls)) {
                        string relativeUrl;
                        while (null != (relativeUrl = urlReader.ReadLine())) {
                            if(String.IsNullOrWhiteSpace(relativeUrl)) {
                                continue;
                            }
                            if (!Uri.IsWellFormedUriString(relativeUrl, UriKind.Relative) || !(relativeUrl.StartsWith("/"))) {
                                AddModelError("Urls", T("\"{0}\" is an invalid warmup url.", relativeUrl));
                            }
                        }
                    }
                }
            }

            if (viewModel.Settings.Scheduled) {
                if (viewModel.Settings.Delay <= 0) {
                    AddModelError("Delay", T("Delay must be greater than zero."));
                }
            }

            if (viewModel.Settings.UseSiteMap) {
                if (string.IsNullOrWhiteSpace(viewModel.Settings.SiteMapUrl)) {
                    AddModelError("SiteMapUrl", T("Sitemap url is required."));
                }
            }

            if (ModelState.IsValid) {
                _warmupScheduler.Schedule(true);
                Services.Notifier.Information(T("Warmup updated successfully."));
            }
            else {
                Services.TransactionManager.Cancel();
            }

            return Index();
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }

}