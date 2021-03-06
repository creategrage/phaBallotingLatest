//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace phaBalloting.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Member
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Member()
        {
            this.Ballotings = new HashSet<Balloting>();
            this.WaitingMembers = new HashSet<WaitingMember>();
        }
    
        public int Id { get; set; }
        public string OldMFormNo { get; set; }
        public string NameOfOfficer { get; set; }
        public string FatherName { get; set; }
        public string HusbandName { get; set; }
        public string Cnic { get; set; }
        public string OfficeName { get; set; }
        public string OfficeStatus { get; set; }
        public System.DateTime DateOfJoiningService { get; set; }
        public string PostHeld { get; set; }
        public string OccutionalGroup { get; set; }
        public System.DateTime DateOfBirth { get; set; }
        public string OfficeAddress { get; set; }
        public string PermanentAddress { get; set; }
        public string HomeTelephone { get; set; }
        public string EmailAddress { get; set; }
        public string ImageUrl { get; set; }
        public string OfficeTelephone { get; set; }
        public string Mobile { get; set; }
        public int BPSId { get; set; }
        public string UserId { get; set; }
        public System.DateTime EntryDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModfiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Balloting> Ballotings { get; set; }
        public virtual BPSList BPSList { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WaitingMember> WaitingMembers { get; set; }
    }
}
