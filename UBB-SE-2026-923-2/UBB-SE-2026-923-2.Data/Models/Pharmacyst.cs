namespace UBB_SE_2026_923_2.Models
{
    /// <summary>
    /// Pharmacist staff. Stored in the same table as <see cref="Staff"/> via TPH
    /// inheritance; the discriminator is <see cref="Staff.Role"/> = "Pharmacist".
    /// All persisted fields (including <see cref="Staff.Certification"/>) live
    /// on the base class.
    /// </summary>
    public class Pharmacyst : Staff
    {
        public Pharmacyst()
        {
            this.Role = "Pharmacist";
        }

        public Pharmacyst(int staffID, string firstName, string lastName, string contactInfo, bool available, string certification, int yearsOfExp)
        {
            this.StaffID = staffID;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.ContactInfo = contactInfo;
            this.Available = available;
            this.Certification = certification;
            this.YearsOfExperience = yearsOfExp;
            this.Role = "Pharmacist";
        }
    }
}
