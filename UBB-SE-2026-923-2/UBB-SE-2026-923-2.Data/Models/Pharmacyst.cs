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

        public Pharmacyst(int staffId, string firstName, string lastName, string contactInformation, bool isAvailable, string certification, int yearsOfExperience)
        {
            this.StaffID = staffId;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.ContactInfo = contactInformation;
            this.Available = isAvailable;
            this.Certification = certification;
            this.YearsOfExperience = yearsOfExperience;
            this.Role = "Pharmacist";
        }
    }
}