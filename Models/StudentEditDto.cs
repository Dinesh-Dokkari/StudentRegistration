namespace StudentRegistration.Models
{
    public class StudentEditDto : StudentUploadDto
    {
        public int AdmissionNo { set; get; }
        public string? ExistingImagePath { get; set; }
        public string? ExistingFilePath { get; set; }
    }
}
