using System.Threading.Tasks;

namespace backend.Services
{
    public interface IQRCodeStatisticService
    {
        Task RecordScanAsync(string qrCode);
        Task<QRCodeStatistic?> GetStatisticsAsync(int qrCodeId);
    }
}
