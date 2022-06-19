using System;
using System.Threading.Tasks;
using Academia.Application;
using Academia.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Academia.Controllers
{
    public static class StudentModule
    {
        public static void MapStudentModule(this WebApplication app)
        {
            app.MapGet("api/students/{studentId:Guid}", GetStudent)
                .Transactional();

            app.MapPost("api/students/~/enroll", EnrollStudent)
                .Transactional();

            app.MapPost("api/students/{studentId:Guid}/update-email", UpdateStudentEmail)
                .Transactional();
        }

        private static async Task<IResult> GetStudent([FromRoute] Guid studentId, [FromServices] StudentApplicationService studentService)
        {
            var student = await studentService.StudentEntity(studentId);

            var studentDto = MapStudentToDto(student);

            return Results.Json(studentDto);
        }

        private static async Task<IResult> EnrollStudent([FromBody] EnrollStudentInput input, [FromServices] StudentApplicationService studentService)
        {
            var student = await studentService.EnrollStudent(input.Name, input.Email, input.Address);

            var studentDto = MapStudentToDto(student);

            return Results.Json(studentDto);
        }

        private static async Task<IResult> UpdateStudentEmail([FromRoute] Guid studentId, [FromBody] ChangeEmailInput input, [FromServices] StudentApplicationService studentService)
        {
            var student = await studentService.ChangeStudentEmail(studentId, input.Email);

            var studentDto = MapStudentToDto(student);

            return Results.Json(studentDto);
        }

        private static StudentDto MapStudentToDto(Student student)
        {
            return new StudentDto
            {
                StudentId = student.Id,
                Name = student.Name,
                Email = student.Email,
                Address = student.Address
            };
        }
    }
}