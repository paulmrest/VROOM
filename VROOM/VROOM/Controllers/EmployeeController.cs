﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VROOM.Models;
using VROOM.Models.DTOs;
using VROOM.Models.Interfaces;

namespace VROOM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {

        private readonly IEmployee _employee;

        public EmployeeController(IEmployee employee)
        {
            _employee = employee;
        }

        // GET: api/employee
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDTO>>> GetEmployees()
        {
            return await _employee.GetEmployees();
        }

        // GET: api/employee/3
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDTO>> GetSingleEmployee(int id)
        {
            EmployeeDTO employeeDTO = await _employee.GetSingleEmployee(id);
            return employeeDTO;
        }

        // PUT: api/employee/3
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]

        public async Task<IActionResult> UpdateEmployee(int id, EmployeeDTO employeeDTO)
        {
            if (id != employeeDTO.Id)
            {
                return BadRequest();
            }

            EmployeeDTO updatedEmployeeDTO = await _employee.UpdateEmployee(id, employeeDTO);

            return Ok(updatedEmployeeDTO);
        }

        // POST: api/employee
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<EmployeeDTO>> CreateEmployee(EmployeeDTO employeeDTO)
        {
            await _employee.CreateEmployee(employeeDTO);

            return CreatedAtAction("GetEmployee", new { id = employeeDTO.Id }, employeeDTO);
        }

        // DELETE: api/employee/3
        [HttpDelete("{id}")]
        public async Task <ActionResult<Employee>> DeleteEmployee(int id)
        {
            await _employee.DeleteEmployee(id);
            return NoContent();
        }

    }
}