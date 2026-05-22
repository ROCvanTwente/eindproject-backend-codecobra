using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Data;

namespace backend.Services
{
    public class QRCodeStatisticService : IQRCodeStatisticService
    {
        private readonly AppDbContext _context;

        public QRCodeStatisticService(AppDbContext context)
        {
            _context = context;
        }

        public async Task RecordScanAsync(string qrCode)
        {
            var qrCodeEntity = await _context.QRCodes
                .FirstOrDefaultAsync(q => q.Code == qrCode);

            if (qrCodeEntity != null)
            {
                var statistic = await _context.QRCodeStatistics
                    .FirstOrDefaultAsync(s => s.QRCodeId == qrCodeEntity.Id);

                if (statistic != null)
                {
                    statistic.ScanCount++;
                    statistic.LastScannedAt = DateTime.UtcNow;
                }
                else
                {
                    statistic = new QRCodeStatistic
                    {
                        QRCodeId = qrCodeEntity.Id,
                        ScanCount = 1,
                        LastScannedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.QRCodeStatistics.Add(statistic);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<QRCodeStatistic?> GetStatisticsAsync(int qrCodeId)
        {
            return await _context.QRCodeStatistics
                .FirstOrDefaultAsync(s => s.QRCodeId == qrCodeId);
        }
    }
}
