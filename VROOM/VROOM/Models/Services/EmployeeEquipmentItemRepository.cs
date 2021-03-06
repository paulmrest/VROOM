using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VROOM.Models.Interfaces;
using VROOM.Data;
using VROOM.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace VROOM.Models.Services
{
    public class EmployeeEquipmentItemRepository : IEmployeeEquipmentItem
    {
        private VROOMDbContext _context;
        private IEmployee _employee;
        private IEquipmentItem _equipmentItem;

        /// <summary>
        /// Instantiates a new EmployeeEquipmentItemRepository object.
        /// </summary>
        /// <param name="context">
        /// VROOMDbContext: an object that inherits from DbContext
        /// </param>
        /// <param name="employee">
        /// IEmployee: an object that implements IEmployee
        /// </param>
        /// <param name="equipmentItem">
        /// IEquipmentItem: an object that implements IEquipmentItem
        /// </param>
        public EmployeeEquipmentItemRepository(VROOMDbContext context, IEmployee employee, IEquipmentItem equipmentItem)
        {
            _context = context;
            _employee = employee;
            _equipmentItem = equipmentItem;
        }

        public async Task<List<EmployeeEquipmentItemDTO>> GetAllEmployeeEquipmentRecords()
        {
            List<EmployeeEquipmentItemDTO> EEItemDTOs = await _context.EmployeeEquipmentItem
                .Include(x => x.Employee)
                .Include(x => x.EquipmentItem)
                .Select(x => new EmployeeEquipmentItemDTO
                {
                    EmployeeId = x.EmployeeId,
                    EquipmentItemId = x.EquipmentItemId,
                    StatusId = x.StatusId,
                    Status = EmployeeEquipmentStatusStringFrom(x.StatusId),
                    DateBorrowed = x.DateBorrowed,
                    DateRecordClosed = x.DateRecordClosed
                })
                .ToListAsync();
            if (EEItemDTOs != null)
            {
                EEItemDTOs = await NestDTOsIn(EEItemDTOs);
            }
            return EEItemDTOs;
        }

        public async Task<List<EmployeeEquipmentItemDTO>> GetAllEmployeeEquipmentRecordsForEmployee(int employeeId)
        {
            List<EmployeeEquipmentItemDTO> oneEmployeeEEItemsDTOs = await _context.EmployeeEquipmentItem
                .Where(x => x.EmployeeId == employeeId)
                .Include(x => x.Employee)
                .Include(x => x.EquipmentItem)
                .Select(x => new EmployeeEquipmentItemDTO
                {
                    EmployeeId = x.EmployeeId,
                    EquipmentItemId = x.EquipmentItemId,
                    StatusId = x.StatusId,
                    Status = EmployeeEquipmentStatusStringFrom(x.StatusId),
                    DateBorrowed = x.DateBorrowed,
                    DateRecordClosed = x.DateRecordClosed
                })
                .ToListAsync();
            if (oneEmployeeEEItemsDTOs != null)
            {
                oneEmployeeEEItemsDTOs = await NestDTOsIn(oneEmployeeEEItemsDTOs);
            }
            return oneEmployeeEEItemsDTOs;
        }

        public async Task<List<EmployeeEquipmentItemDTO>> GetAllEmployeeEquipmentRecordsForEquipmentItem(int equipmentItemId)
        {
            List<EmployeeEquipmentItemDTO> oneItemEEItemsDTOs = await _context.EmployeeEquipmentItem
                .Where(x => x.EquipmentItemId == equipmentItemId)
                .Include(x => x.Employee)
                .Include(x => x.EquipmentItem)
                .Select(x => new EmployeeEquipmentItemDTO
                {
                    EmployeeId = x.EmployeeId,
                    EquipmentItemId = x.EquipmentItemId,
                    StatusId = x.StatusId,
                    Status = EmployeeEquipmentStatusStringFrom(x.StatusId),
                    DateBorrowed = x.DateBorrowed,
                    DateRecordClosed = x.DateRecordClosed
                })
                .ToListAsync();
            if (oneItemEEItemsDTOs != null)
            {
                oneItemEEItemsDTOs = await NestDTOsIn(oneItemEEItemsDTOs);
            }
            return oneItemEEItemsDTOs;
        }

        public async Task<List<EmployeeEquipmentItemDTO>> GetAllEmployeeEquipmentRecordsFor(int employeeId, int equipmentItemId)
        {
            List<EmployeeEquipmentItemDTO> oneEmployeeAndItemEEItemsDTOs = await _context.EmployeeEquipmentItem
                .Where(x => x.EmployeeId == employeeId)
                .Where(x => x.EquipmentItemId == equipmentItemId)
                .Include(x => x.Employee)
                .Include(x => x.EquipmentItem)
                .Select(x => new EmployeeEquipmentItemDTO
                {
                    EmployeeId = x.EmployeeId,
                    EquipmentItemId = x.EquipmentItemId,
                    StatusId = x.StatusId,
                    Status = EmployeeEquipmentStatusStringFrom(x.StatusId),
                    DateBorrowed = x.DateBorrowed,
                    DateRecordClosed = x.DateRecordClosed
                })
                .ToListAsync();
            if (oneEmployeeAndItemEEItemsDTOs != null)
            {
                oneEmployeeAndItemEEItemsDTOs = await NestDTOsIn(oneEmployeeAndItemEEItemsDTOs);
            }
            return oneEmployeeAndItemEEItemsDTOs;
        }

        public async Task<List<EmployeeEquipmentItemDTO>> GetAllEmployeeEquipmentRecordsWith(EmployeeEquipmentStatus status)
        {
            List<EmployeeEquipmentItemDTO> EEItemsDTOsWithStatus = await _context.EmployeeEquipmentItem
                .Where(x => x.StatusId == (int)status)
                .Include(x => x.Employee)
                .Include(x => x.EquipmentItem)
                .Select(x => new EmployeeEquipmentItemDTO
                {
                    EmployeeId = x.EmployeeId,
                    EquipmentItemId = x.EquipmentItemId,
                    StatusId = x.StatusId,
                    Status = EmployeeEquipmentStatusStringFrom(x.StatusId),
                    DateBorrowed = x.DateBorrowed,
                    DateRecordClosed = x.DateRecordClosed
                })
                .ToListAsync();
            if (EEItemsDTOsWithStatus != null)
            {
                EEItemsDTOsWithStatus = await NestDTOsIn(EEItemsDTOsWithStatus);
            }
            return EEItemsDTOsWithStatus;
        }

        public async Task<EmployeeEquipmentItemDTO> SetEquipmentItemAsBorrowedBy(int employeeId, EmployeeEquipmentItemDTO EEItemDTO)
        {
            if (!await CheckIfItemIsAvailable(EEItemDTO.EquipmentItemId))
            {
                //client is asking to borrow a piece of equipment not available, but there's no way of knowing that record's CKs
                return null;
            }
            EEItemDTO.StatusId = (int)EmployeeEquipmentStatus.Borrowed;
            EEItemDTO.DateBorrowed = DateTime.Now;
            EmployeeEquipmentItem EEItem = ConvertFromDTOtoEntity(EEItemDTO);
            EEItem.RecordStatusId = (int)EmployeeEquipmentRecordStatus.Open;
            _context.Entry(EEItem).State = EntityState.Added;
            await _context.SaveChangesAsync();
            EEItemDTO.Status = ((EmployeeEquipmentStatus)EEItemDTO.StatusId).ToString();
            var EEItemDTOWithEquipmentItem = await AddEquipmentItem(EEItemDTO);
            return EEItemDTOWithEquipmentItem;
        }

        public async Task<EmployeeEquipmentItemDTO> UpdateEmployeeEquipmentItemRecord(EmployeeEquipmentItemDTO EEItemDTO)
        {
            EmployeeEquipmentItem EEItem = await _context.FindAsync<EmployeeEquipmentItem>(EEItemDTO.EmployeeId, EEItemDTO.EquipmentItemId, EEItemDTO.DateBorrowed);
            if (EEItem.StatusId == (int)EmployeeEquipmentStatus.Returned)
            {
                return ConvertFromEntityToDTO(EEItem);
            }
            if (EEItemDTO.StatusId == (int)EmployeeEquipmentStatus.Returned || 
                EEItemDTO.StatusId == (int)EmployeeEquipmentStatus.Destroyed || 
                EEItemDTO.StatusId == (int)EmployeeEquipmentStatus.Sold)
            {
                EEItemDTO.DateRecordClosed = DateTime.Now;
                EEItem.DateRecordClosed = EEItemDTO.DateRecordClosed;
                EEItem.RecordStatusId = (int)EmployeeEquipmentRecordStatus.Closed;
            }
            EEItemDTO.Status = EmployeeEquipmentStatusStringFrom(EEItemDTO.StatusId);
            EEItem.StatusId = EEItemDTO.StatusId;
            _context.Entry(EEItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            var EEItemDTOWithEquipmentItem = await AddEquipmentItem(EEItemDTO);
            return EEItemDTOWithEquipmentItem;
        }

        public async Task<EmployeeEquipmentItemDTO> ReturnItem(EmployeeEquipmentItemDTO EEItemDTO)
        {
            EEItemDTO.StatusId = (int)EmployeeEquipmentStatus.Returned;
            return await UpdateEmployeeEquipmentItemRecord(EEItemDTO);
        }

        public async Task<EmployeeEquipmentItemDTO> GetReturnableItem(int employeeId, int equipmentItemId)
        {
            //get most recent record for item
            var mostRecentActivityItem = await _context.EmployeeEquipmentItem
                .Where(x => x.EmployeeId == employeeId)
                .Where(x => x.EquipmentItemId == equipmentItemId)
                .OrderByDescending(x => x.DateBorrowed)
                .FirstOrDefaultAsync();
            //if item has no record, it is not returnable
            if (mostRecentActivityItem == null ||
                (mostRecentActivityItem.StatusId != (int)EmployeeEquipmentStatus.Borrowed ||
                 mostRecentActivityItem.RecordStatusId != (int)EmployeeEquipmentRecordStatus.Open))
            {
                return null;
            }
            else
            {
                //if most recent record shows it as being borrowed, and is open, item is returnable
                return ConvertFromEntityToDTO(mostRecentActivityItem);
            }
        }

        public async Task<EmployeeEquipmentItemDTO> GetUpdatableItem(int employeeId, int equipmentItemId)
        {
            //get most recent record for item
            var mostRecentActivityItem = await _context.EmployeeEquipmentItem
                .Where(x => x.EmployeeId == employeeId)
                .Where(x => x.EquipmentItemId == equipmentItemId)
                .OrderByDescending(x => x.DateBorrowed)
                .FirstOrDefaultAsync();
            //if item has no record, it is not returnable
            if (mostRecentActivityItem == null)
            {
                return null;
            }
            //if most recent record shows it as sold or destroyed, not updatable
            else if (mostRecentActivityItem.StatusId == (int)EmployeeEquipmentStatus.Destroyed ||
                     mostRecentActivityItem.StatusId == (int)EmployeeEquipmentStatus.Sold)
            {
                return null;
            }
            //if most recent record has been closed, not updatable
            else if (mostRecentActivityItem.RecordStatusId == (int)EmployeeEquipmentRecordStatus.Closed)
            {
                return null;
            }
            else
            {
                //if most recent record shows it as being borrowed, and is open, item is returnable
                return ConvertFromEntityToDTO(mostRecentActivityItem);
            }
        }

        public async Task<bool> CheckIfItemIsAvailable(int equipmentItemId)
        {
            var EItemDTO = await _equipmentItem.GetEquipmentItem(equipmentItemId);
            //if item doesn't exist at all, it is not available
            if (EItemDTO == null)
            {
                return false;
            }
            //get most recent returned record for item
            var mostRecentActivityItem = await _context.EmployeeEquipmentItem
                .Where(x => x.EquipmentItemId == equipmentItemId)
                .OrderByDescending(x => x.DateBorrowed)
                .FirstOrDefaultAsync();
            //if item has no record, it is available to be borrowed
            if (mostRecentActivityItem == null)
            {
                return true;
            }
            else
            {
                //if most recent record has been closed, item is available to be borrowed
                return (mostRecentActivityItem.StatusId == (int)EmployeeEquipmentStatus.Returned && 
                        mostRecentActivityItem.RecordStatusId == (int)EmployeeEquipmentRecordStatus.Closed);
            }
        }

        /// <summary>
        /// Private helper method. Adds Employee and EquipmentItem DTOs to EmployeeEquipmentItem DTOs as nested objects.
        /// </summary>
        /// <param name="EEItemDTOs">
        /// List<EmployeeEquiptmentItemDTO>: a List of EmployeeEquipmentItemDTOs
        /// </param>
        /// <returns>
        /// List<EmployeeEquiptmentItemDTO>: a List of EmployeeEquipmentItemDTOs with Employee and EquipmentItem DTOs embedded
        /// </returns>
        private async Task<List<EmployeeEquipmentItemDTO>> NestDTOsIn(List<EmployeeEquipmentItemDTO> EEItemDTOs)
        {
            foreach (EmployeeEquipmentItemDTO EEIDTO in EEItemDTOs)
            {
                EEIDTO.Employee = await _employee.GetSingleEmployee(EEIDTO.EmployeeId);
                EEIDTO.EquipmentItem = await _equipmentItem.GetEquipmentItem(EEIDTO.EquipmentItemId);
            }
            return EEItemDTOs;
        }

        /// <summary>
        /// Inserts the corresponding EquipmentItemDTO into an EmployeeEquipmentItemDTO and returns that EmployeeEquipmentItemDTO.
        /// </summary>
        /// <param name="EEItemDTO">
        /// EmployeeEquipmentItemDTO: an EmployeeEquipmentItemDTO missing an embedded EquipmentItemDTO
        /// </param>
        /// <returns>
        /// EmployeeEquipmentItemDTO: an EmployeeEquipmentItemDTO with its corresponding EquipmentItemDTO embedded
        /// </returns>
        private async Task<EmployeeEquipmentItemDTO> AddEquipmentItem(EmployeeEquipmentItemDTO EEItemDTO)
        {
            EEItemDTO.EquipmentItem = await _equipmentItem.GetEquipmentItem(EEItemDTO.EquipmentItemId);
            return EEItemDTO;
        }

        /// <summary>
        /// Private helper method. Converts from EmployeeEquipmentItemDTO to EmployeeEquipmentItem (entity) object.
        /// </summary>
        /// <param name="EEItemDTO">
        /// EmployeeEquipmentItemDTO: a DTO object
        /// </param>
        /// <returns>
        /// EmployeeEquipmentItem: an entity object
        /// </returns>
        private EmployeeEquipmentItem ConvertFromDTOtoEntity(EmployeeEquipmentItemDTO EEItemDTO)
        {
            return new EmployeeEquipmentItem()
            {
                EmployeeId = EEItemDTO.EmployeeId,
                EquipmentItemId = EEItemDTO.EquipmentItemId,
                StatusId = EEItemDTO.StatusId,
                DateBorrowed = EEItemDTO.DateBorrowed,
                DateRecordClosed = EEItemDTO.DateRecordClosed,
            };
        }

        /// <summary>
        /// Private helper method. Converts from EmployeeEquipmentItem (entity) to EmployeeEquipmentItemDTO object.
        /// </summary>
        /// <param name="EEItem">
        /// EmployeeEquipmentItem: an entity object
        /// </param>
        /// <returns>
        /// EmployeeEquipmentItemDTO: a DTO object
        /// </returns>
        private EmployeeEquipmentItemDTO ConvertFromEntityToDTO(EmployeeEquipmentItem EEItem)
        {
            return new EmployeeEquipmentItemDTO()
            {
                EmployeeId = EEItem.EmployeeId,
                EquipmentItemId = EEItem.EquipmentItemId,
                StatusId = EEItem.StatusId,
                DateBorrowed = EEItem.DateBorrowed,
                DateRecordClosed = EEItem.DateRecordClosed,
            };
        }

        /// <summary>
        /// Private helper method. Returns a string form of the EmployeeEquipmentStatus enum.
        /// </summary>
        /// <param name="statusId">
        /// int: the int form of the EmployeeEquipmentStatus enum
        /// </param>
        /// <returns>
        /// string: the string form of the EmployeeEquipmentStatus enum
        /// </returns>
        private static string EmployeeEquipmentStatusStringFrom(int statusId)
        {
            return ((EmployeeEquipmentStatus)statusId).ToString();
        }
    }
}