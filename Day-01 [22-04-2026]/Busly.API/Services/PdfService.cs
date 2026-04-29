using Busly.API.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Busly.API.Services;

public class PdfService : IPdfService
{
    public PdfService()
    {
        global::QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenerateTicketAsync(Booking booking)
    {
        var pnr = booking.Id.ToString("N")[..8].ToUpper();

        var bus = booking.Bus;
        var busName = bus?.BusName ?? "N/A";
        var busNumber = bus?.BusNumber ?? "N/A";
        var operatorName = bus?.Operator?.CompanyName ?? "N/A";

        var boardingStop = bus?.BusStops
            .Where(s => s.Type == "BOARDING")
            .FirstOrDefault();

        var droppingStop = bus?.BusStops
            .Where(s => s.Type == "DROPPING")
            .FirstOrDefault();

        var boardingCity = boardingStop?.City ?? "N/A";
        var boardingAddress = boardingStop?.Address ?? "N/A";
        var boardingTime = boardingStop?.ScheduledTime?.ToString("hh\\:mm") ?? "N/A";

        var droppingCity = droppingStop?.City ?? "N/A";
        var droppingAddress = droppingStop?.Address ?? "N/A";
        var droppingTime = droppingStop?.ScheduledTime?.ToString("hh\\:mm") ?? "N/A";

        // Collect seat details from booked seats
        var seatDetails = booking.BookedSeats
            .Select(bs => new
            {
                PassengerName = bs.PassengerName ?? "N/A",
                PassengerAge = bs.PassengerAge?.ToString() ?? "N/A",
                PassengerGender = bs.PassengerGender ?? "N/A",
                SeatNumber = bs.Seat?.SeatNumber?.ToString() ?? "N/A",
                Deck = bs.Seat?.Deck ?? "N/A"
            })
            .ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                // Header
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Busly")
                            .FontSize(28)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);
                        col.Item().Text("Your Journey, Our Commitment")
                            .FontSize(10)
                            .Italic()
                            .FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(120).AlignRight().Column(col =>
                    {
                        col.Item().Text("E-TICKET")
                            .FontSize(16)
                            .Bold()
                            .FontColor(Colors.Grey.Darken2);
                        col.Item().Text($"PNR: {pnr}")
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Blue.Medium);
                    });
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    // Divider
                    col.Item().LineHorizontal(1).LineColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(10);

                    // Bus & Operator Info
                    col.Item().Background(Colors.Blue.Lighten5).Padding(8).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Bus Details").Bold().FontSize(12);
                            c.Item().Text($"Bus Name: {busName}");
                            c.Item().Text($"Number Plate: {busNumber}");
                            c.Item().Text($"Operator: {operatorName}");
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Journey Details").Bold().FontSize(12);
                            c.Item().Text($"Journey Date: {booking.JourneyDate}");
                            c.Item().Text($"Status: {booking.Status}");
                        });
                    });

                    col.Item().PaddingTop(10);

                    // Boarding & Dropping
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(8).Column(c =>
                        {
                            c.Item().Text("Boarding Point").Bold().FontColor(Colors.Green.Darken2);
                            c.Item().Text(boardingCity).FontSize(13).Bold();
                            c.Item().Text(boardingAddress).FontSize(10).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"Time: {boardingTime}").FontSize(10);
                        });

                        row.ConstantItem(20).AlignMiddle().AlignCenter()
                            .Text("→").FontSize(18).Bold().FontColor(Colors.Blue.Medium);

                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(8).Column(c =>
                        {
                            c.Item().Text("Dropping Point").Bold().FontColor(Colors.Red.Darken2);
                            c.Item().Text(droppingCity).FontSize(13).Bold();
                            c.Item().Text(droppingAddress).FontSize(10).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"Time: {droppingTime}").FontSize(10);
                        });
                    });

                    col.Item().PaddingTop(10);

                    // Passenger Table
                    col.Item().Text("Passenger Details").Bold().FontSize(12);
                    col.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        // Header row
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                .Text("Name").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                .Text("Age").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                .Text("Gender").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                .Text("Seat").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                .Text("Deck").Bold().FontColor(Colors.White);
                        });

                        // Data rows
                        foreach (var (seat, index) in seatDetails.Select((s, i) => (s, i)))
                        {
                            var bg = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                            table.Cell().Background(bg).Padding(5).Text(seat.PassengerName);
                            table.Cell().Background(bg).Padding(5).Text(seat.PassengerAge);
                            table.Cell().Background(bg).Padding(5).Text(seat.PassengerGender);
                            table.Cell().Background(bg).Padding(5).Text(seat.SeatNumber);
                            table.Cell().Background(bg).Padding(5).Text(seat.Deck);
                        }
                    });

                    col.Item().PaddingTop(10);

                    // Fare Summary
                    col.Item().AlignRight().Column(c =>
                    {
                        c.Item().Text("Fare Summary").Bold().FontSize(12);
                        c.Item().Text($"Base Fare: {booking.BaseFare:C}");
                        c.Item().Text($"Convenience Fee: {booking.ConvenienceFee:C}");
                        c.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        c.Item().Text($"Total Amount Paid: {booking.TotalAmount:C}")
                            .Bold().FontSize(13).FontColor(Colors.Green.Darken2);
                    });
                });

                // Footer
                page.Footer().AlignCenter().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(4).Text(text =>
                    {
                        text.Span("Busly").Bold().FontColor(Colors.Blue.Darken2);
                        text.Span(" — Thank you for travelling with us. Have a safe journey!");
                        text.Span($"  |  Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC")
                            .FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                });
            });
        });

        var bytes = document.GeneratePdf();
        return Task.FromResult(bytes);
    }
}
