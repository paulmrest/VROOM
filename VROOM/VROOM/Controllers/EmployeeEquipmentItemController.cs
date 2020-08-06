﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VROOM.Data;
using VROOM.Models;
using VROOM.Models.Interfaces;
using VROOM.Models.DTOs;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace VROOM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeEquipmentItemController : ControllerBase
    {
        private readonly IEmployeeEquipmentItem _employeeEquipmentItem;

        private IEmailSender _emailSenderService;

        private IConfiguration _config;

        public EmployeeEquipmentItemController(IEmployeeEquipmentItem employeeEquipmentItem, IEmailSender emailSenderService, IConfiguration config)
        {
            _employeeEquipmentItem = employeeEquipmentItem;
            _emailSenderService = emailSenderService;
            _config = config;
        }

        // GET: api/EmployeeEquipmentItem
        [HttpGet]
        [Authorize(Policy = "BronzeLevel")]
        public async Task<ActionResult<IEnumerable<EmployeeEquipmentItemDTO>>> GetEmployeeEquipmentItems()
        {
            var EEItems = await _employeeEquipmentItem.GetAllEmployeeEquipmentRecords();
            if (EEItems == null)
            {
                return NotFound();
            }
            return EEItems;
        }

        // GET: api/EmployeeEquipmentItem/Employee/{employeeId}
        [HttpGet("employee/{employeeId}")]
        [Authorize(Policy = "BronzeLevel")]
        public async Task<ActionResult<IEnumerable<EmployeeEquipmentItemDTO>>> GetEmployeeEquipmentItemForEmployee(int employeeId)
        {
            var EEItemsForEmployee = await _employeeEquipmentItem.GetAllEmployeeEquipmentRecordsForEmployee(employeeId);
            if (EEItemsForEmployee == null)
            {
                return NotFound();
            }
            return EEItemsForEmployee;
        }

        // GET: api/EmployeeEquipmentItem/EquipmentItem/{equipmentId}
        [HttpGet("equipmentitem/{equipmentItemId}")]
        [Authorize(Policy = "BronzeLevel")]
        public async Task<ActionResult<IEnumerable<EmployeeEquipmentItemDTO>>> GetAllEmployeeEquipmentRecordsForEquipmentItem(int equipmentItemId)
        {
            var EEItemsForEquipmentItem = await _employeeEquipmentItem.GetAllEmployeeEquipmentRecordsForEquipmentItem(equipmentItemId);
            if (EEItemsForEquipmentItem == null)
            {
                return NotFound();
            }
            return EEItemsForEquipmentItem;
        }

        // GET: api/EmployeeEquipmentItem/Employee/{employeeId}/EquipmentItem/{equipmentId}
        [HttpGet("employee/{employeeId}/equipmentitem/{equipmentItemId}")]
        [Authorize(Policy = "BronzeLevel")]
        public async Task<ActionResult<IEnumerable<EmployeeEquipmentItemDTO>>> GetAllEmployeeEquipmentRecordsFor(int employeeId, int equipmentItemId)
        {
            var EEItemsForEmployeeAndEItem = await _employeeEquipmentItem.GetAllEmployeeEquipmentRecordsFor(employeeId, equipmentItemId);
            if (EEItemsForEmployeeAndEItem == null)
            {
                return NotFound();
            }
            return EEItemsForEmployeeAndEItem;
        }

        // GET: api/EmployeeEquipmentItem/Status/{statusId}
        [HttpGet("status/{statusId}")]
        [Authorize(Policy = "BronzeLevel")]
        public async Task<ActionResult<IEnumerable<EmployeeEquipmentItemDTO>>> GetAllEmployeeEquipmentRecordsWithStatus(int statusId)
        {
            EmployeeEquipmentStatus status = (EmployeeEquipmentStatus)statusId;
            var EEItemsWithStatus = await _employeeEquipmentItem.GetAllEmployeeEquipmentRecordsWith(status);
            if (EEItemsWithStatus == null)
            {
                return NotFound();
            }
            return EEItemsWithStatus;
        }

        // POST: api/EmployeeEquipmentItem
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost("{employeeId}")]
        [Authorize(Policy = "SilverLevel")]
        public async Task<ActionResult<EmployeeEquipmentItemDTO>> SetEquipmentItemAsBorrowedBy(int employeeId, EmployeeEquipmentItemDTO EEItemDTO)
        {
            if (employeeId != EEItemDTO.EmployeeId)
            {
                return BadRequest("EmployeeIDs must match.");
            }
            if (!await _employeeEquipmentItem.CheckIfItemIsAvailable(EEItemDTO.EquipmentItemId))
            {
                return BadRequest("Item is not available to be borrowed.");
            }
            else
            {
                var updatedEEItemDTO = await _employeeEquipmentItem.SetEquipmentItemAsBorrowedBy(employeeId, EEItemDTO);
                SendNotificationEmail(updatedEEItemDTO);
                return updatedEEItemDTO;
            }
        }

        // PUT: api/EmployeeEquipmentItem/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{equipmentItemId}")]
        [Authorize(Policy = "BronzeLevel")]
        public async Task<ActionResult<EmployeeEquipmentItemDTO>> UpdateEmployeeEquipmentItem(int equipmentItemId, EmployeeEquipmentItemDTO EEItemDTO)
        {
            if (equipmentItemId != EEItemDTO.EquipmentItemId)
            {
                return BadRequest("EquipmentItemIDs must match.");
            }
            if (await _employeeEquipmentItem.CheckIfItemIsAvailable(equipmentItemId))
            {
                return BadRequest("No updatable items found for that EquipmentItemID and EmployeeID combination.");
            }
            else
            {
                //var EEItemDTOToBeUpdated = returnAbleItemForEmployeeDTOs.First();
                //EEItemDTOToBeUpdated.StatusId = EEItemDTO.StatusId;
                //var updatedEEItemDTO = await _employeeEquipmentItem.UpdateEmployeeEquipmentItemRecord(EEItemDTOToBeUpdated);
                //SendNotificationEmail(updatedEEItemDTO);
                //return updatedEEItemDTO;
                return null;
            }
        }

        // PUT: api/EmployeeEquipmentItem/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("/Employee/{employeeId}/ReturnItem/{equipmentItemId}/")]
        [Authorize(Policy = "BronzeLevel")]
        public async Task<ActionResult<EmployeeEquipmentItemDTO>> ReturnEquipmentItem(int employeeId, int equipmentItemId)
        {
            var returnAbleItemForEmployeeDTOs = await _employeeEquipmentItem.ListOfUpdatableItemsFor(employeeId, equipmentItemId);
            //for a given EmployeeID-EquipmentItemID combination, there should only ever be 0 or 1 returnable items
            if (returnAbleItemForEmployeeDTOs.Count < 1)
            {
                return BadRequest("Entered data does not match any returnable items.");
            }
            else if (returnAbleItemForEmployeeDTOs.Count > 1)
            {
                return BadRequest("Database state invalid. More than one returnable item found for EquipmentItemID and EmployeeID combination.");
            }
            else
            {
                var EEItemDTOToBeReturned = returnAbleItemForEmployeeDTOs.First();
                var updatedEEItemDTO = await _employeeEquipmentItem.ReturnItem(EEItemDTOToBeReturned);
                return updatedEEItemDTO;
            }
        }

        private void SendNotificationEmail(EmployeeEquipmentItemDTO EEItemDTO)
        {
            string equipmentItemName = EEItemDTO.EquipmentItem.Name;
            string userEmail = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            string sendEmail = _config["CorporateEmail"];
            string firstName = User.Claims.First(x => x.Type == "FirstName").Value;
            string lastName = User.Claims.First(x => x.Type == "LastName").Value;
            switch (EEItemDTO.StatusId)
            {
                case (int)EmployeeEquipmentStatus.Borrowed:
                    SendBorrowedNotificationEmail(equipmentItemName, userEmail, sendEmail, firstName, lastName);
                    break;
                case (int)EmployeeEquipmentStatus.Returned:
                    SendReturnedNotificationEmail(equipmentItemName, userEmail, sendEmail, firstName, lastName);
                    break;
                default:
                    break;
            }
        }

        private async void SendBorrowedNotificationEmail(string equipmentItemName, string userEmail, string sendEmail, string firstName, string lastName)
        {
            string emailSubject = $"Equipment Notification - {equipmentItemName} Borrowed by {userEmail}";
            string emailHTMLBody = $"<p>This message is for {firstName} {lastName}</p><p>You've been marked in our Employee Equipment Item Manager (EEIM) system as having borrowed a {equipmentItemName}.</p><p>If you believe this is in error, please contact your manager.";
            await _emailSenderService.SendEmailAsync(sendEmail, emailSubject, emailHTMLBody);
        }

        private async void SendReturnedNotificationEmail(string equipmentItemName, string userEmail, string sendEmail, string firstName, string lastName)
        {
            string emailSubject = $"Equipment Notification - {equipmentItemName} Returned by {userEmail}";
            string emailHTMLBody = $"<p>This message is for {firstName} {lastName}</p><p>You've been marked in our Employee Equipment Item Manager (EEIM) system as having returned a {equipmentItemName}.</p><p>If you believe this is in error, please contact your manager.";
            await _emailSenderService.SendEmailAsync(sendEmail, emailSubject, emailHTMLBody);
        }
    }
}
