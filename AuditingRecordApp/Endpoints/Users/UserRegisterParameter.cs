namespace AuditingRecordApp.Endpoints.Users;

public record UserRegisterParameter(
    string FirstName, 
    string LastName, 
    string UserName,
    string Email, 
    string Password);