using System;
using System.Collections.Generic;

namespace PharmacyManagementSystem.Web.Models
{
    public class User
    {
        public int Id { get; set; }

        public int PharmacyId { get; set; }

        public string Username { get; set; } = "";

        public string PasswordHash { get; set; } = "";

        public string Role { get; set; } = "Staff";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public string PharmacyName { get; set; } = "";

        // ============================================================
        // USER PERMISSIONS
        // ============================================================

        public List<string> Permissions { get; set; }
            = new List<string>();
    }
}