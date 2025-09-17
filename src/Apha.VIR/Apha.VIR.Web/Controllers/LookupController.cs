using System.ComponentModel.DataAnnotations;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Models.Lookup;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Controllers
{
    public class LookupController : Controller
    {
        private readonly ILookupService _lookupService;
        private readonly IMapper _mapper;

        public LookupController(ILookupService lookupService, IMapper mapper)
        {
            _lookupService = lookupService;
            _mapper = mapper;
        }

        [Authorize(Roles = AppRoleConstant.LookupDataManager)]
        public async Task<IActionResult> Index()
        {
            var result = await _lookupService.GetAllLookupsAsync();

            var lookups = _mapper.Map<IEnumerable<LookupViewModel>>(result);

            return View("Lookup", lookups);
        }

        [Authorize(Roles = AppRoleConstant.LookupDataManager)]
        public async Task<IActionResult> LookupList(Guid lookupid, int pageNo = 1, int pageSize = 10)
        {
            if (lookupid == Guid.Empty || !ModelState.IsValid)
            {
                return BadRequest("Invalid parameters.");
            }

            var lookupResult = await _lookupService.GetLookupByIdAsync(lookupid);
            var lookup = _mapper.Map<LookupViewModel>(lookupResult);

            var lookupEntries = await _lookupService.GetAllLookupItemsAsync(lookupid, pageNo, pageSize);
            var lookupItems = _mapper.Map<IEnumerable<LookupItemModel>>(lookupEntries.data);

            var viewModel = new LookupListViewModel
            {
                LookupName = lookup.Name + " Look-up List",
                LookupId = lookup.Id,
                IsReadOnly = lookup.ReadOnly,
                LookupItemResult = new LookupItemListViewModel
                {
                    LookupId = lookup.Id,
                    ShowParent = lookup.Parent == Guid.Empty || lookup.Parent == null ? false : true,
                    ShowAlternateName = lookup.AlternateName,
                    ShowSMSRelated = lookup.Smsrelated,
                    LookupItems = lookupItems.ToList(),
                    Pagination = new PaginationModel
                    {
                        PageNumber = pageNo,
                        PageSize = pageSize,
                        TotalCount = lookupEntries.TotalCount
                    }
                }
            };

            return View("LookupItem", viewModel);
        }

        [Authorize(Roles = AppRoleConstant.LookupDataManager)]
        public async Task<IActionResult> BindLookupItemGridOnPagination(Guid lookupid, int pageNo, int pageSize)
        {
            if (lookupid == Guid.Empty || !ModelState.IsValid)
            {
                return BadRequest("Invalid parameters.");
            }

            var lookupResult = await _lookupService.GetLookupByIdAsync(lookupid);
            var lookup = _mapper.Map<LookupViewModel>(lookupResult);

            var lookupEntries = await _lookupService.GetAllLookupItemsAsync(lookupid, pageNo, pageSize);
            var lookups = _mapper.Map<IEnumerable<LookupItemModel>>(lookupEntries.data);

            var LookupItemResult = new LookupItemListViewModel
            {
                LookupId = lookup.Id,
                ShowParent = lookup.Parent == Guid.Empty || lookup.Parent == null ? false : true,
                ShowAlternateName = lookup.AlternateName,
                ShowSMSRelated = lookup.Smsrelated,
                LookupItems = lookups.ToList(),
                Pagination = new PaginationModel
                {
                    PageNumber = pageNo,
                    PageSize = pageSize,
                    TotalCount = lookupEntries.TotalCount
                }
            };

            return PartialView("_LookupItemList", LookupItemResult);
        }

        [HttpGet]
        [Authorize(Roles = AppRoleConstant.LookupDataManager)]
        public async Task<IActionResult> Create(Guid lookupId, int currentPage = 1)
        {
            if (lookupId == Guid.Empty || !ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid parameters.");
                return BadRequest(ModelState);
            }

            var lookupResult = await _lookupService.GetLookupByIdAsync(lookupId);
            var lookup = _mapper.Map<LookupViewModel>(lookupResult);

            var viewModel = new LookupItemViewModel
            {
                LookupId = lookup.Id,
                ShowParent = lookup.Parent != Guid.Empty && lookup.Parent.HasValue ? true : false,
                ShowAlternateName = lookup.AlternateName,
                ShowSMSRelated = lookup.Smsrelated,
                IsReadOnly = lookup.ReadOnly,
                LookupParentList = lookup.Parent != Guid.Empty && lookup.Parent.HasValue
                ? GetLookupItemPresents(lookup).Result.Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = f.Name
                }).ToList()
                : null,
                LookupItem = new LookupItemModel(),
                ShowErrorSummary = false
            };

            return View("CreateLookupItem", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(LookupItemViewModel model)
        {
            if (!AuthorisationUtil.CanAddItem(AppRoleConstant.LookupDataManager))
            {
                var lookupResult = await _lookupService.GetLookupByIdAsync(model.LookupId);
                var lookup = _mapper.Map<LookupViewModel>(lookupResult);
                throw new UnauthorizedAccessException("Not authorised to insert entry in " + lookup?.Name + " list.");
            }

            ModelState.Remove("LookupItem.LastModified");

            var showerrorSummary = false;

            if (ModelState.IsValid)
            {
                await ValidateModel(model, ModelState, "create");
                showerrorSummary = true;
            }

            if (!ModelState.IsValid)
            {
                if (model.ShowParent)
                {
                    var lookupResult = await _lookupService.GetLookupByIdAsync(model.LookupId);
                    var lookup = _mapper.Map<LookupViewModel>(lookupResult);
                    model.LookupParentList = GetLookupItemPresents(lookup).Result.Select(f => new SelectListItem
                    {
                        Value = f.Id.ToString(),
                        Text = f.Name
                    }).ToList();
                }
                model.ShowErrorSummary = showerrorSummary;

                return View("CreateLookupItem", model);
            }

            var dto = _mapper.Map<LookupItemDto>(model.LookupItem);

            await _lookupService.InsertLookupItemAsync(model.LookupId, dto);

            return RedirectToAction(nameof(LookupList), new { lookupid = model.LookupId, pageNo = 1, pageSize = 10 });
        }

        [HttpGet]
        [Authorize(Roles = AppRoleConstant.LookupDataManager)]
        public async Task<IActionResult> Edit(Guid lookupId, Guid lookupItemId, int currentPage = 1)
        {
            if (lookupId == Guid.Empty || lookupItemId == Guid.Empty || !ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid parameters.");
                return BadRequest(ModelState);
            }

            var lookupResult = await _lookupService.GetLookupByIdAsync(lookupId);
            var lookup = _mapper.Map<LookupViewModel>(lookupResult);

            var lookupItemResult = await _lookupService.GetLookupItemAsync(lookupId, lookupItemId);
            var lookupItem = _mapper.Map<LookupItemModel>(lookupItemResult);

            var viewModel = new LookupItemViewModel
            {
                LookupId = lookup.Id,
                ShowParent = lookup.Parent != Guid.Empty && lookup.Parent.HasValue ? true : false,
                ShowAlternateName = lookup.AlternateName,
                ShowSMSRelated = lookup.Smsrelated,
                IsReadOnly = lookup.ReadOnly,
                LookupParentList = lookup.Parent != Guid.Empty && lookup.Parent.HasValue
                ? GetLookupItemPresents(lookup).Result.Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = f.Name
                }).ToList()
                : null,
                LookupItem = lookupItem,
                ShowErrorSummary = false
            };

            return View("EditLookupItem", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(LookupItemViewModel model)
        {
            var showerrorSummary = false;

            if (!AuthorisationUtil.CanEditItem(AppRoleConstant.LookupDataManager))
            {
                var lookupResult = await _lookupService.GetLookupByIdAsync(model.LookupId);
                var lookup = _mapper.Map<LookupViewModel>(lookupResult);
                throw new UnauthorizedAccessException("Not authorised to update entry in " + lookup?.Name + " list.");
            }

            if (ModelState.IsValid)
            {
                await ValidateModel(model, ModelState, "edit");
                showerrorSummary = true;
            }

            if (!ModelState.IsValid)
            {
                if (model.ShowParent)
                {
                    var lookupResult = await _lookupService.GetLookupByIdAsync(model.LookupId);
                    var lookup = _mapper.Map<LookupViewModel>(lookupResult);
                    model.LookupParentList = GetLookupItemPresents(lookup).Result.Select(f => new SelectListItem
                    {
                        Value = f.Id.ToString(),
                        Text = f.Name
                    }).ToList();
                }
                model.ShowErrorSummary = showerrorSummary;

                return View("EditLookupItem", model);
            }

            var dto = _mapper.Map<LookupItemDto>(model.LookupItem);

            await _lookupService.UpdateLookupItemAsync(model.LookupId, dto);

            return RedirectToAction(nameof(LookupList), new { lookupid = model.LookupId, pageNo = 1, pageSize = 10 });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(LookupItemViewModel model)
        {
            var showerrorSummary = false;

            if (!AuthorisationUtil.CanDeleteItem(AppRoleConstant.LookupDataManager))
            {
                var lookupResult = await _lookupService.GetLookupByIdAsync(model.LookupId);
                var lookup = _mapper.Map<LookupViewModel>(lookupResult);
                throw new UnauthorizedAccessException("Not authorised to delete entry in " + lookup?.Name + " list.");
            }

            if (ModelState.IsValid)
            {
                await ValidateModel(model, ModelState, "delete");
                showerrorSummary = true;
            }

            if (!ModelState.IsValid)
            {
                if (model.ShowParent)
                {
                    var lookupResult = await _lookupService.GetLookupByIdAsync(model.LookupId);
                    var lookup = _mapper.Map<LookupViewModel>(lookupResult);
                    model.LookupParentList = GetLookupItemPresents(lookup).Result.Select(f => new SelectListItem
                    {
                        Value = f.Id.ToString(),
                        Text = f.Name
                    }).ToList();
                }

                model.ShowErrorSummary = showerrorSummary;

                return View("EditLookupItem", model);
            }

            var dto = _mapper.Map<LookupItemDto>(model.LookupItem);

            await _lookupService.DeleteLookupItemAsync(model.LookupId, dto);

            return RedirectToAction(nameof(LookupList), new { lookupid = model.LookupId, pageNo = 1, pageSize = 10 });
        }

        private async Task<IEnumerable<LookupItemModel>> GetLookupItemPresents(LookupViewModel lookup)
        {
            if (lookup.Parent.HasValue && lookup.Parent != Guid.Empty)
            {
                Guid parentId = lookup.Parent.Value;
                var lookupParentResult = await _lookupService.GetLookupItemParentListAsync(parentId);
                var lookupParentList = _mapper.Map<IEnumerable<LookupItemModel>>(lookupParentResult);
                return lookupParentList;
            }
            return new List<LookupItemModel>();
        }

        private async Task ValidateModel(LookupItemViewModel model, ModelStateDictionary modelState, string action)
        {
            bool IsitemInUse = false;
            var context = new ValidationContext(model.LookupItem);

            var lookupEntries = await _lookupService.GetAllLookupItemsAsync(model.LookupId);
            var lookupItems = _mapper.Map<IEnumerable<LookupItemModel>>(lookupEntries);

            IsitemInUse = await GetItemInUse(model, action, IsitemInUse);

            var validationResult = GetValidationResult(model, action, IsitemInUse, context, lookupItems);

            foreach (var validation in validationResult)
            {
                foreach (var memberName in validation.MemberNames.Any() ? validation.MemberNames : new[] { "" })
                {
                    if (validation.ErrorMessage != null)
                        modelState.AddModelError(memberName, validation.ErrorMessage);
                }
            }
        }

        private async Task<bool> GetItemInUse(LookupItemViewModel model, string action, bool IsitemInUse)
        {
            switch (action)
            {
                case "create":
                    return false;

                case "edit":
                    if (!model.LookupItem.Active)
                    {
                        return await _lookupService.IsLookupItemInUseAsync(model.LookupId, model.LookupItem.Id);
                    }
                    return false;

                case "delete":
                    return await _lookupService.IsLookupItemInUseAsync(model.LookupId, model.LookupItem.Id);

                default:
                    return IsitemInUse;
            }
        }

        static IEnumerable<ValidationResult> GetValidationResult(LookupItemViewModel model,
                string action, bool IsitemInUse, ValidationContext context, IEnumerable<LookupItemModel> lookupItems)
        {
            return action switch
            {
                "create" => model.LookupItem.ValidateLookUpItemAdd(context, lookupItems, model.ShowParent),
                "edit" => model.LookupItem.ValidateLookUpItemUpdate(context, lookupItems, model.ShowParent, IsitemInUse),
                "delete" => model.LookupItem.ValidateLookUpItemDelete(context, lookupItems, IsitemInUse),
                _ => Enumerable.Empty<ValidationResult>(),
            };

        }
    }
}
