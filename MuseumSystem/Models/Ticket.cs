using Avalonia.Media.Imaging;
using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MuseumSystem.Models;

public partial class Ticket
{
    public int Id { get; set; }

    public string Number { get; set; } = null!;

    public int? TypeId { get; set; }

    public int UserId { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public DateOnly ValidFrom { get; set; }

    public DateOnly ValidTo { get; set; }

    public decimal Price { get; set; }

    public bool? IsUsed { get; set; }

    public virtual ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();

    public virtual TicketType? Type { get; set; }

    public virtual User User { get; set; } = null!;

    public Bitmap qrBitmap
    {
        get
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode($"Номер: {Number}, Клиент: {User.FullName}, Дата покупки: {PurchaseDate}, Доступен с: {ValidFrom}, Доступен до: {ValidTo}, Тип {Type.Name}, Доступные мероприятия: {EventRegistrations.Select(s => s.Event.Title)}", QRCodeGenerator.ECCLevel.Q);
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                using (var ms = new MemoryStream(qrCodeImage))
                {
                    return new Bitmap(ms);
                }

            };

        }
    }


}
