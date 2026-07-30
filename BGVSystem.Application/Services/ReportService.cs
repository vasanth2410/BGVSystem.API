using BGVSystem.Application.Interfaces;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BGVSystem.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IVerificationRepository _verificationRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IEmailService _emailService;

        public ReportService(
            ICandidateRepository candidateRepository,
            IVerificationRepository verificationRepository = null,
            IDocumentRepository documentRepository = null,
            IEmailService emailService = null)
        {
            _candidateRepository = candidateRepository;
            _verificationRepository = verificationRepository;
            _documentRepository = documentRepository;
            _emailService = emailService;
        }

        public async Task<byte[]> ExportCandidatesAsync()
        {
            var candidates = await _candidateRepository.GetAllAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Candidates");

            worksheet.Cell(1, 1).Value = "Id";
            worksheet.Cell(1, 2).Value = "Full Name";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Status";

            var row = 2;
            foreach (var candidate in candidates)
            {
                worksheet.Cell(row, 1).Value = candidate.Id;
                worksheet.Cell(row, 2).Value = candidate.FullName;
                worksheet.Cell(row, 3).Value = candidate.Email;
                worksheet.Cell(row, 4).Value = candidate.Status;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> GenerateCandidatePdfReportAsync(int candidateId)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var candidate = await _candidateRepository.GetByIdAsync(candidateId);
            if (candidate == null)
            {
                throw new KeyNotFoundException($"Candidate with ID {candidateId} not found");
            }

            var verifications = _verificationRepository != null 
                ? await _verificationRepository.GetByCandidateIdAsync(candidateId) 
                : new List<Domain.Entities.Verification>();

            var documents = _documentRepository != null 
                ? await _documentRepository.GetByCandidateIdAsync(candidateId) 
                : new List<Domain.Entities.Document>();

            var pdfDocument = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // HEADER
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("BGV SYSTEM").FontSize(22).Bold().FontColor("#1e3a8a");
                            col.Item().Text("BACKGROUND VERIFICATION SUMMARY REPORT").FontSize(10).SemiBold().FontColor("#475569");
                        });

                        row.ConstantItem(180).Column(col =>
                        {
                            col.Item().AlignRight().Text($"REPORT REF: BGV-{candidate.Id:D6}").FontSize(9).Bold().FontColor("#0f172a");
                            col.Item().AlignRight().Text($"GENERATED: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(8).FontColor("#64748b");
                            col.Item().AlignRight().Text($"CONFIDENTIALITY: STRICTLY PRIVATE").FontSize(8).Bold().FontColor("#dc2626");
                        });
                    });

                    // CONTENT
                    page.Content().PaddingVertical(15).Column(column =>
                    {
                        // Candidate Profile Box
                        column.Item().Background("#f8fafc").Border(1).BorderColor("#cbd5e1").Padding(12).Column(profileCol =>
                        {
                            profileCol.Item().Text("CANDIDATE INFORMATION").FontSize(11).Bold().FontColor("#1e293b");
                            profileCol.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text(t => { t.Span("Full Name: ").Bold(); t.Span(candidate.FullName ?? "-"); });
                                    col.Item().Text(t => { t.Span("Email Address: ").Bold(); t.Span(candidate.Email ?? "-"); });
                                    col.Item().Text(t => { t.Span("Phone Number: ").Bold(); t.Span(candidate.PhoneNumber ?? "-"); });
                                    col.Item().Text(t => { t.Span("Gender: ").Bold(); t.Span(candidate.Gender ?? "-"); });
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text(t => { t.Span("Applied Role: ").Bold(); t.Span(candidate.AppliedRole ?? "-"); });
                                    col.Item().Text(t => { t.Span("PAN Number: ").Bold(); t.Span(candidate.PANNumber ?? "-"); });
                                    col.Item().Text(t => { t.Span("Aadhaar Number: ").Bold(); t.Span(candidate.AadhaarNumber ?? "-"); });
                                    col.Item().Text(t => { t.Span("Overall Status: ").Bold(); t.Span(candidate.Status ?? "Pending").FontColor(candidate.Status == "Approved" ? "#16a34a" : candidate.Status == "Rejected" ? "#dc2626" : "#d97706").Bold(); });
                                });
                            });
                        });

                        column.Item().PaddingTop(15).Text("VERIFICATION CHECKS BREAKDOWN").FontSize(11).Bold().FontColor("#1e293b");

                        // Verifications Table
                        column.Item().PaddingTop(6).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(30);
                                cols.RelativeColumn(3);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(4);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#0f172a").Padding(6).Text("#").Bold().FontColor(Colors.White).FontSize(9);
                                header.Cell().Background("#0f172a").Padding(6).Text("Verification Check").Bold().FontColor(Colors.White).FontSize(9);
                                header.Cell().Background("#0f172a").Padding(6).Text("Status").Bold().FontColor(Colors.White).FontSize(9);
                                header.Cell().Background("#0f172a").Padding(6).Text("Reviewer Remarks").Bold().FontColor(Colors.White).FontSize(9);
                            });

                            int idx = 1;
                            var defaultTypes = new[] { "Identity Verification", "Document Check", "Education Verification", "Employment Verification", "Criminal Record Check" };
                            
                            foreach (var checkType in defaultTypes)
                            {
                                var existing = verifications.FirstOrDefault(v => v.VerificationType != null && v.VerificationType.Equals(checkType, StringComparison.OrdinalIgnoreCase));
                                string status = existing?.Status ?? "In Progress";
                                string remarks = existing?.ReviewerRemarks ?? "Verification check initiated.";

                                string statusColor = status == "Approved" || status == "Cleared" ? "#16a34a" : status == "Rejected" ? "#dc2626" : "#d97706";

                                table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(6).Text($"{idx++}").FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(6).Text(checkType).SemiBold().FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(6).Text(status).Bold().FontColor(statusColor).FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(6).Text(remarks).FontSize(9);
                            }
                        });

                        // Verified Documents Section
                        if (documents.Any())
                        {
                            column.Item().PaddingTop(15).Text("VERIFIED DOCUMENTS ATTACHED").FontSize(11).Bold().FontColor("#1e293b");
                            column.Item().PaddingTop(6).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(3);
                                    cols.RelativeColumn(4);
                                    cols.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background("#334155").Padding(6).Text("Document Type").Bold().FontColor(Colors.White).FontSize(9);
                                    header.Cell().Background("#334155").Padding(6).Text("File Name").Bold().FontColor(Colors.White).FontSize(9);
                                    header.Cell().Background("#334155").Padding(6).Text("Status").Bold().FontColor(Colors.White).FontSize(9);
                                });

                                foreach (var doc in documents)
                                {
                                    table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(6).Text(doc.FileType ?? "Document").FontSize(9);
                                    table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(6).Text(doc.FileName ?? "-").FontSize(9);
                                    table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(6).Text(doc.Status ?? "Uploaded").FontSize(9);
                                }
                            });
                        }

                        // OFFICIAL STAMP / FOOTER BOX
                        column.Item().PaddingTop(25).Border(1).BorderColor("#2563eb").Background("#eff6ff").Padding(12).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("OFFICIAL VERIFICATION CERTIFICATE").FontSize(10).Bold().FontColor("#1e40af");
                                col.Item().Text("This document certifies that the background verification process for the above candidate has been conducted in accordance with BGV System Compliance Guidelines.").FontSize(8).FontColor("#3b82f6");
                            });

                            row.ConstantItem(120).AlignRight().Column(col =>
                            {
                                col.Item().Text("STATUS").FontSize(8).Bold().FontColor("#64748b");
                                col.Item().Text(candidate.Status == "Approved" ? "VERIFIED VALID" : candidate.Status == "Rejected" ? "VERIFICATION FAILED" : "PENDING AUDIT").FontSize(9).Bold().FontColor(candidate.Status == "Approved" ? "#16a34a" : "#dc2626");
                            });
                        });
                    });

                    // FOOTER
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                        x.Span(" | BGV System Automated PDF Verification Report");
                    });
                });
            });

            var pdfBytes = pdfDocument.GeneratePdf();

            if (_emailService != null && candidate != null)
            {
                try
                {
                    await _emailService.SendPdfReportReadyEmailAsync(
                        candidate.Email,
                        candidate.FullName,
                        $"BGV-{candidate.Id:D6}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EMAIL NOTICE] SendPdfReportReadyEmailAsync failed: {ex.Message}");
                }
            }

            return pdfBytes;
        }
    }
}
