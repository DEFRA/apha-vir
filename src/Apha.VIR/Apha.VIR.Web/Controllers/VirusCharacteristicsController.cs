using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Models.VirusCharacteristic;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Controllers
{
    public class VirusCharacteristicsController : Controller
    {
        private readonly IVirusCharacteristicService _virusCharacteristicService;
        private readonly IMapper _mapper;

        public VirusCharacteristicsController(IVirusCharacteristicService virusCharacteristicService, IMapper mapper)
        {
            _virusCharacteristicService = virusCharacteristicService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!AuthorisationUtil.IsUserInAnyRole())
            {
                return RedirectToAction(nameof(AccountController.AccessDenied), "Account");
            }
            return View("VirusCharacteristicManagement");
        }

        [HttpGet]
        public async Task<IActionResult> List(int pageNo = 1, int pageSize = 10)
        {
            if (!AuthorisationUtil.IsUserInAnyRole())
            {
                return RedirectToAction(nameof(AccountController.AccessDenied), "Account");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync(pageNo, pageSize);

            var virusCharacteristics = _mapper.Map<List<VirusCharacteristicsModel>>(result.data);

            var viewmodel = new VirusCharacteristicsViewModel
            {
                List = virusCharacteristics,
                Pagination = new PaginationModel
                {
                    PageNumber = pageNo,
                    PageSize = pageSize,
                    TotalCount = result.TotalCount
                }
            };

            return View(viewmodel);
        }

        [HttpGet]
        public async Task<IActionResult> BindCharacteristicsGridOnPagination(int pageNo, int pageSize)
        {
            if (!AuthorisationUtil.IsUserInAnyRole())
            {
                return RedirectToAction(nameof(AccountController.AccessDenied), "Account");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync(pageNo, pageSize);

            var virusCharacteristics = _mapper.Map<List<VirusCharacteristicsModel>>(result.data);

            var viewmodel = new VirusCharacteristicsViewModel
            {
                List = virusCharacteristics,
                Pagination = new PaginationModel
                {
                    PageNumber = pageNo,
                    PageSize = pageSize,
                    TotalCount = result.TotalCount
                }
            };

            return PartialView("_VirusCharatersticsList", viewmodel);
        }

        [HttpGet]
        public async Task<IActionResult> CreateAsync()
        {
            if (!AuthorisationUtil.IsUserInAnyRole())
            {
                return RedirectToAction(nameof(AccountController.AccessDenied), "Account");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var virusTypesDto = await _virusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync();

            var virusTypes = _mapper.Map<List<VirusCharacteristicDataType>>(virusTypesDto);

            var virusTypesSelectList = virusTypes == null ? new List<SelectListItem>() : virusTypes.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.DataType
            }).ToList();

            var viewmodel = new VirusCharacteristicsModel
            {
                CharacteristicTypeNameList = virusTypesSelectList,
            };

            return View(viewmodel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(VirusCharacteristicsModel model)
        {
            ViewBag.showsummary = false;

            if (!AuthorisationUtil.CanAddItem(AppRoleConstant.LookupDataManager))
            {
                throw new UnauthorizedAccessException("Not authorised to insert entry in VirusCharacteristic list.");
            }

            if (ModelState.IsValid)
            {
                var validationErrors = await ValidateVirusCharacteristicAdd(model);

                if (validationErrors.Count > 0)
                {
                    ViewBag.showsummary = true;
                    foreach (var error in validationErrors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var virusTypesDto = await _virusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync();

                var virusTypes = _mapper.Map<List<VirusCharacteristicDataType>>(virusTypesDto);

                var virusTypesSelectList = virusTypes.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.DataType
                }).ToList();

                var viewmodel = new VirusCharacteristicsModel
                {
                    CharacteristicTypeNameList = virusTypesSelectList,
                };

                return View(viewmodel);
            }

            var dto = _mapper.Map<VirusCharacteristicDto>(model);
            await _virusCharacteristicService.AddEntryAsync(dto);

            return RedirectToAction("List");
        }

        [HttpGet]
        public async Task<IActionResult> EditAsync(Guid id)
        {
            if (!AuthorisationUtil.IsUserInAnyRole())
            {
                return RedirectToAction(nameof(AccountController.AccessDenied), "Account");
            }

            if (id == Guid.Empty || !ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _virusCharacteristicService.GetVirusCharacteristicsByIdAsync(id);
            var viewModel = _mapper.Map<VirusCharacteristicsModel>(result);

            var virusTypesDto = await _virusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync();

            var virusTypes = _mapper.Map<List<VirusCharacteristicDataType>>(virusTypesDto);

            var virusTypesSelectList = virusTypes.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.DataType
            }).ToList();

            viewModel.CharacteristicTypeNameList = virusTypesSelectList;

            return View(viewModel);

        }

        [HttpPost]
        public async Task<IActionResult> Edit(VirusCharacteristicsModel model)
        {
            ViewBag.showsummary = false;

            if (!AuthorisationUtil.CanEditItem(AppRoleConstant.LookupDataManager))
            {
                throw new UnauthorizedAccessException("Not authorised to update entry in VirusCharacteristic list.");
            }

            if (ModelState.IsValid)
            {
                var validationErrors = await ValidateVirusCharacteristicEdit(model);

                if (validationErrors.Count > 0)
                {
                    ViewBag.showsummary = true;
                    foreach (var error in validationErrors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var virusTypesDto = await _virusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync();

                var virusTypes = _mapper.Map<List<VirusCharacteristicDataType>>(virusTypesDto);

                var virusTypesSelectList = virusTypes == null ? new List<SelectListItem>() : virusTypes.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.DataType
                }).ToList();

                model.CharacteristicTypeNameList = virusTypesSelectList;

                return View("Edit", model);
            }

            var dto = _mapper.Map<VirusCharacteristicDto>(model);
            await _virusCharacteristicService.UpdateEntryAsync(dto);

            return RedirectToAction("List");
        }

        public async Task<IActionResult> Delete(VirusCharacteristicsModel model, Guid id)
        {
            ViewBag.showsummary = false;

            if (!AuthorisationUtil.CanDeleteItem(AppRoleConstant.LookupDataManager))
            {
                throw new UnauthorizedAccessException("Not authorised to delete entry in VirusCharacteristic list.");
            }

            if (ModelState.IsValid)
            {
                var validationErrors = await ValidateVirusCharacteristicDelete(model);

                if (validationErrors.Count > 0)
                {
                    ViewBag.showsummary = true;
                    foreach (var error in validationErrors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var virusTypesDto = await _virusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync();

                var virusTypes = _mapper.Map<List<VirusCharacteristicDataType>>(virusTypesDto);

                var virusTypesSelectList = virusTypes == null ? new List<SelectListItem>() : virusTypes.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.DataType
                }).ToList();

                model.CharacteristicTypeNameList = virusTypesSelectList;

                return View("Edit", model);
            }

            await _virusCharacteristicService.DeleteVirusCharactersticsAsync(id, model.LastModified);

            return RedirectToAction(nameof(List));
        }

        private async Task<List<string>> ValidateVirusCharacteristicAdd(VirusCharacteristicsModel model)
        {
            var errors = new List<string>();

            var allCharacteristics = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync();
            if (allCharacteristics.Any(vc => vc.Id == model.Id))
            {
                errors.Add("Item already exists.");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                errors.Add("Name not specified for this item.");
            }

            if (!model.CharacteristicIndex.HasValue)
            {
                errors.Add("Display Order not specified for this item.");
            }

            if (model.Length.HasValue && model.Length.Value > 100)
            {
                errors.Add("Maximum length must be no more than 100 characters.");
            }

            return errors;
        }

        private async Task<List<string>> ValidateVirusCharacteristicEdit(VirusCharacteristicsModel model)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(model.Id.ToString()))
            {
                errors.Add("Id not specified for this item.");
            }

            var allCharacteristics = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync();

            if (!allCharacteristics.Any(vc => vc.Id == model.Id))
            {
                errors.Add("Item does not exist.");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                errors.Add("Name not specified for this item.");
            }

            if (!model.CharacteristicIndex.HasValue)
            {
                errors.Add("Display Order not specified for this item.");
            }

            if (model.Length.HasValue && model.Length.Value > 100)
            {
                errors.Add("Maximum length must be no more than 100 characters.");
            }

            return errors;
        }

        private async Task<List<string>> ValidateVirusCharacteristicDelete(VirusCharacteristicsModel model)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(model.Id.ToString()))
            {
                errors.Add("Id not specified for this item.");
            }

            var allCharacteristics = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync();

            if (!allCharacteristics.Any(vc => vc.Id == model.Id))
            {
                errors.Add("Item does not exist.");
            }

            var inUse = await _virusCharacteristicService.CheckVirusCharactersticsUsageByIdAsync(model.Id);

            if (inUse)
            {
                ModelState.AddModelError("", "Virus Characteristic cannot be deleted as it is already assigned to one or more Virus Isolates.");
            }

            return errors;
        }
    }
}
