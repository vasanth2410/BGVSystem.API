using BGVSystem.Application.Interfaces;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.Services
{
    public class ReportService
    : IReportService
    {
        private readonly
            ICandidateRepository
            _candidateRepository;

        public ReportService(
            ICandidateRepository
                candidateRepository)
        {
            _candidateRepository =
                candidateRepository;
        }

        public async Task<byte[]>
            ExportCandidatesAsync()
        {
            var candidates =
                await _candidateRepository
                    .GetAllAsync();

            using var workbook =
                new XLWorkbook();

            var worksheet =
                workbook.Worksheets
                    .Add("Candidates");

            worksheet.Cell(1, 1)
                .Value = "Id";

            worksheet.Cell(1, 2)
                .Value = "Full Name";

            worksheet.Cell(1, 3)
                .Value = "Email";

            worksheet.Cell(1, 4)
                .Value = "Status";

            var row = 2;

            foreach (var candidate
                in candidates)
            {
                worksheet.Cell(row, 1)
                    .Value =
                    candidate.Id;

                worksheet.Cell(row, 2)
                    .Value =
                    candidate.FullName;

                worksheet.Cell(row, 3)
                    .Value =
                    candidate.Email;

                worksheet.Cell(row, 4)
                    .Value =
                    candidate.Status;

                row++;
            }

            using var stream =
                new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }
    }
}
