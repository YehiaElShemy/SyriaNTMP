namespace SyriaNTMP.Dto
{
    public class FileExportDto
    {
        public FileExportDto(string fileName, byte[] bytes, string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileName = fileName;
            Bytes = bytes;
            ContentType = contentType;
        }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Bytes { get; set; }
    }
}
