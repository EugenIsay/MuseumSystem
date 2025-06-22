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
    public string Massage
    {
        get
        {
            if ((bool)IsUsed)
                return "Оплачено";
            else return "Не оплачено";
        }
    }

    public virtual ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();

    public virtual TicketType? Type { get; set; }

    public virtual User User { get; set; } = null!;

    public byte[] qrBytes
    {
        get
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode($"{Number}", QRCodeGenerator.ECCLevel.Q);
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                return qrCode.GetGraphic(20);
            };
        }
    }

    public Bitmap qrBitmap
    {
        get
        {
            using (var ms = new MemoryStream(qrBytes))
            {
                return new Bitmap(ms);
            }
        }
    }


}
