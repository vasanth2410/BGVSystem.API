using System;
using System.Collections.Generic;
using System.Text;
namespace BGVSystem.Domain.Entities;

public class Candidate
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string PANNumber { get; set; } = string.Empty;

    public string AadhaarNumber { get; set; } = string.Empty;

    public string AppliedRole { get; set; } = string.Empty;

    public DateTime DateOfJoining { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<Document> Documents { get; set; } = new List<Document>();

    public ICollection<Verification> Verifications { get; set; } = new List<Verification>();

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

}