using MarineWorkshopApp.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace MarineWorkshopApp.Services
{
    public static class InvoicePdfService
    {
        public static string GenerateAndSave(
            Invoice invoice,
            Client client,
            CompanySettings settings,
            IEnumerable<InvoiceItem> items,
            string? savePath = null)
        {
            savePath ??= Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                $"بيان_أعمال_{invoice.InvoiceNumber}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));
                    page.ContentFromRightToLeft();

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(settings.WorkshopName).FontSize(18).Bold().FontColor("#0B2559");
                                c.Item().Text(settings.Subtitle).FontSize(10).FontColor("#64748B");
                                if (!string.IsNullOrEmpty(settings.Address))
                                    c.Item().Text(settings.Address).FontSize(9).FontColor("#64748B");
                            });

                            row.ConstantItem(100).AlignLeft().Column(c =>
                            {
                                if (!string.IsNullOrEmpty(settings.LogoPath) && File.Exists(settings.LogoPath))
                                    c.Item().Height(50).Image(settings.LogoPath);
                            });
                        });

                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor("#E2E8F0");

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("بيان أعمال تصنيع وتوريد").FontSize(14).Bold();
                                c.Item().Text($"رقم البيان: {invoice.InvoiceNumber}").FontSize(10);
                                c.Item().Text($"التاريخ: {invoice.Date:yyyy/MM/dd}").FontSize(10);
                            });

                            row.RelativeItem().AlignLeft().Column(c =>
                            {
                                c.Item().Text($"العميل: {client.CompanyName}").Bold();
                                c.Item().Text($"المالك: {client.OwnerName}").FontSize(10);
                                if (!string.IsNullOrEmpty(client.Phone))
                                    c.Item().Text($"الهاتف: {client.Phone}").FontSize(10);
                            });
                        });

                        col.Item().PaddingVertical(8).LineHorizontal(1).LineColor("#E2E8F0");
                    });

                    page.Content().Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#0B2559").Padding(6).Text("البند").FontColor(Colors.White).Bold();
                                header.Cell().Background("#0B2559").Padding(6).Text("المقاسات").FontColor(Colors.White).Bold();
                                header.Cell().Background("#0B2559").Padding(6).Text("العدد").FontColor(Colors.White).Bold();
                                header.Cell().Background("#0B2559").Padding(6).Text("سعر الوحدة").FontColor(Colors.White).Bold();
                                header.Cell().Background("#0B2559").Padding(6).Text("الإجمالي").FontColor(Colors.White).Bold();
                            });

                            foreach (var item in items)
                            {
                                table.Cell().BorderBottom(1).BorderColor("#F1F5F9").Padding(6).Text(item.ItemName);
                                table.Cell().BorderBottom(1).BorderColor("#F1F5F9").Padding(6).Text(item.Dimensions);
                                table.Cell().BorderBottom(1).BorderColor("#F1F5F9").Padding(6).Text(item.Quantity.ToString());
                                table.Cell().BorderBottom(1).BorderColor("#F1F5F9").Padding(6).Text($"{item.UnitPrice:N0} {settings.CurrencySymbol}");
                                table.Cell().BorderBottom(1).BorderColor("#F1F5F9").Padding(6).Text($"{item.TotalPrice:N0} {settings.CurrencySymbol}").Bold();
                            }
                        });

                        col.Item().PaddingTop(20).AlignLeft().Column(totals =>
                        {
                            totals.Item().Text($"الإجمالي الكلي: {invoice.GrandTotal:N2} {settings.CurrencySymbol}")
                                .FontSize(16).Bold().FontColor("#0B2559");
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("شركة أعالي البحار - ").FontSize(9).FontColor("#94A3B8");
                        text.Span(DateTime.Now.ToString("yyyy")).FontSize(9).FontColor("#94A3B8");
                    });
                });
            }).GeneratePdf(savePath);

            return savePath;
        }
    }
}
