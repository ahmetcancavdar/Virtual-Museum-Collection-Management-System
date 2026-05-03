using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualMuseum.Web.Data;
using VirtualMuseum.Web.Models.ViewModels.Reports;
using System.Data;
using System.Text;

namespace VirtualMuseum.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminReportsController : Controller
    {
        private readonly VirtualMuseumDbContext _context;

        public AdminReportsController(VirtualMuseumDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ActiveExhibitionArtworkCoverage()
        {
            var rows = new List<ActiveExhibitionArtworkCoverageRowViewModel>();

            var sql = @"
SELECT
    E.Exhibition_ID,
    E.Title AS Exhibition_Title,
    E.Room_ID,
    E.Room_Name,
    AW.Artwork_ID,
    AW.Title AS Artwork_Title,
    AW.Artwork_type,
    AW.Theme,
    A.Name + ' ' + A.Surname AS Artist_Name
FROM EXHIBITION E
INNER JOIN FEATURES F
    ON E.Exhibition_ID = F.Exhibition_ID
INNER JOIN ARTWORK AW
    ON F.Artwork_ID = AW.Artwork_ID
INNER JOIN ARTIST A
    ON AW.Artist_ID = A.Artist_ID
WHERE E.Is_Active = 1
ORDER BY
    E.Exhibition_ID,
    AW.Artwork_ID;";

            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = sql;

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    rows.Add(new ActiveExhibitionArtworkCoverageRowViewModel
                    {
                        ExhibitionId = reader.GetInt32(0),
                        ExhibitionTitle = reader.GetString(1),
                        RoomId = reader.GetInt32(2),
                        RoomName = reader.GetString(3),
                        ArtworkId = reader.GetInt32(4),
                        ArtworkTitle = reader.GetString(5),
                        ArtworkType = reader.GetString(6),
                        Theme = reader.GetString(7),
                        ArtistName = reader.GetString(8)
                    });
                }
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    await connection.CloseAsync();
            }

            return View(rows);
        }
        public async Task<IActionResult> ExportActiveExhibitionArtworkCoverageHtml()
        {
            var rows = new List<ActiveExhibitionArtworkCoverageRowViewModel>();

            var sql = @"
SELECT
    E.Exhibition_ID,
    E.Title AS Exhibition_Title,
    E.Room_ID,
    E.Room_Name,
    AW.Artwork_ID,
    AW.Title AS Artwork_Title,
    AW.Artwork_type,
    AW.Theme,
    A.Name + ' ' + A.Surname AS Artist_Name
FROM EXHIBITION E
INNER JOIN FEATURES F
    ON E.Exhibition_ID = F.Exhibition_ID
INNER JOIN ARTWORK AW
    ON F.Artwork_ID = AW.Artwork_ID
INNER JOIN ARTIST A
    ON AW.Artist_ID = A.Artist_ID
WHERE E.Is_Active = 1
ORDER BY
    E.Exhibition_ID,
    AW.Artwork_ID;";

            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = sql;

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    rows.Add(new ActiveExhibitionArtworkCoverageRowViewModel
                    {
                        ExhibitionId = reader.GetInt32(0),
                        ExhibitionTitle = reader.GetString(1),
                        RoomId = reader.GetInt32(2),
                        RoomName = reader.GetString(3),
                        ArtworkId = reader.GetInt32(4),
                        ArtworkTitle = reader.GetString(5),
                        ArtworkType = reader.GetString(6),
                        Theme = reader.GetString(7),
                        ArtistName = reader.GetString(8)
                    });
                }
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    await connection.CloseAsync();
            }

            string EscapeHtml(string? value)
            {
                if (string.IsNullOrEmpty(value))
                    return string.Empty;

                return System.Net.WebUtility.HtmlEncode(value);
            }

            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\" />");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
            sb.AppendLine("<title>Active Exhibition Artwork Coverage</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 24px; }");
            sb.AppendLine("h1 { margin-bottom: 8px; }");
            sb.AppendLine("p { color: #555; margin-bottom: 20px; }");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; }");
            sb.AppendLine("th, td { border: 1px solid #ccc; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background-color: #f3f3f3; }");
            sb.AppendLine("tr:nth-child(even) { background-color: #fafafa; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<h1>Active Exhibition Artwork Coverage</h1>");
            sb.AppendLine("<p>This report lists all artworks currently displayed in active exhibitions together with exhibition and room information.</p>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Exhibition ID</th>");
            sb.AppendLine("<th>Exhibition Title</th>");
            sb.AppendLine("<th>Room ID</th>");
            sb.AppendLine("<th>Room Name</th>");
            sb.AppendLine("<th>Artwork ID</th>");
            sb.AppendLine("<th>Artwork Title</th>");
            sb.AppendLine("<th>Artwork Type</th>");
            sb.AppendLine("<th>Theme</th>");
            sb.AppendLine("<th>Artist Name</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            foreach (var row in rows)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{row.ExhibitionId}</td>");
                sb.AppendLine($"<td>{EscapeHtml(row.ExhibitionTitle)}</td>");
                sb.AppendLine($"<td>{row.RoomId}</td>");
                sb.AppendLine($"<td>{EscapeHtml(row.RoomName)}</td>");
                sb.AppendLine($"<td>{row.ArtworkId}</td>");
                sb.AppendLine($"<td>{EscapeHtml(row.ArtworkTitle)}</td>");
                sb.AppendLine($"<td>{EscapeHtml(row.ArtworkType)}</td>");
                sb.AppendLine($"<td>{EscapeHtml(row.Theme)}</td>");
                sb.AppendLine($"<td>{EscapeHtml(row.ArtistName)}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            return File(bytes, "text/html", "ActiveExhibitionArtworkCoverage.html");
        }

        public async Task<IActionResult> MostRepresentedArtMovements()
        {
            var rows = new List<MostRepresentedArtMovementRowViewModel>();

            var sql = @"
SELECT
    M.Movement_ID,
    M.Movement_name,
    M.Era,
    COUNT(B.Artwork_ID) AS Artwork_Count
FROM ART_MOVEMENT M
LEFT JOIN BELONGS_TO B
    ON M.Movement_ID = B.Movement_ID
LEFT JOIN ARTWORK AW
    ON B.Artwork_ID = AW.Artwork_ID
GROUP BY
    M.Movement_ID,
    M.Movement_name,
    M.Era
ORDER BY
    Artwork_Count DESC,
    M.Movement_name ASC;";

            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = sql;

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    rows.Add(new MostRepresentedArtMovementRowViewModel
                    {
                        MovementId = reader.GetInt32(0),
                        MovementName = reader.GetString(1),
                        Era = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        ArtworkCount = reader.GetInt32(3)
                    });
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }

            return View(rows);
        }
        public async Task<IActionResult> ExportMostRepresentedArtMovementsHtml()
        {
            var rows = new List<MostRepresentedArtMovementRowViewModel>();

            var sql = @"
SELECT
    M.Movement_ID,
    M.Movement_name,
    M.Era,
    COUNT(B.Artwork_ID) AS Artwork_Count
FROM ART_MOVEMENT M
LEFT JOIN BELONGS_TO B
    ON M.Movement_ID = B.Movement_ID
LEFT JOIN ARTWORK AW
    ON B.Artwork_ID = AW.Artwork_ID
GROUP BY
    M.Movement_ID,
    M.Movement_name,
    M.Era
ORDER BY
    Artwork_Count DESC,
    M.Movement_name ASC;";

            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = sql;

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    rows.Add(new MostRepresentedArtMovementRowViewModel
                    {
                        MovementId = reader.GetInt32(0),
                        MovementName = reader.GetString(1),
                        Era = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        ArtworkCount = reader.GetInt32(3)
                    });
                }
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    await connection.CloseAsync();
            }

            string EscapeHtml(string? value)
            {
                if (string.IsNullOrEmpty(value))
                    return string.Empty;

                return System.Net.WebUtility.HtmlEncode(value);
            }

            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\" />");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
            sb.AppendLine("<title>Most Represented Art Movements</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 24px; }");
            sb.AppendLine("h1 { margin-bottom: 8px; }");
            sb.AppendLine("p { color: #555; margin-bottom: 20px; }");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; }");
            sb.AppendLine("th, td { border: 1px solid #ccc; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background-color: #f3f3f3; }");
            sb.AppendLine("tr:nth-child(even) { background-color: #fafafa; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<h1>Most Represented Art Movements</h1>");
            sb.AppendLine("<p>This report shows how strongly each art movement is represented in the museum collection.</p>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Movement ID</th>");
            sb.AppendLine("<th>Movement Name</th>");
            sb.AppendLine("<th>Era</th>");
            sb.AppendLine("<th>Artwork Count</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            foreach (var row in rows)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{row.MovementId}</td>");
                sb.AppendLine($"<td>{EscapeHtml(row.MovementName)}</td>");
                sb.AppendLine($"<td>{EscapeHtml(row.Era)}</td>");
                sb.AppendLine($"<td>{row.ArtworkCount}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            return File(bytes, "text/html", "MostRepresentedArtMovements.html");
        }

        public async Task<IActionResult> VirtualTourStopSequence()
        {
            var rows = new List<VirtualTourStopSequenceRowViewModel>();

            var sql = @"
SELECT
    E.Exhibition_ID,
    E.Title AS Exhibition_Title,
    P.Plan_ID,
    P.Title AS Plan_Title,
    S.Step_No,
    S.Room_ID,
    S.Room_Name,
    S.Stop_Title,
    S.Notes,
    S.Estimated_Minutes
FROM VIRTUAL_TOUR_PLAN P
INNER JOIN EXHIBITION E
    ON P.Exhibition_ID = E.Exhibition_ID
INNER JOIN VIRTUAL_TOUR_STOP S
    ON P.Plan_ID = S.Plan_ID
ORDER BY
    P.Plan_ID,
    S.Step_No;";

            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = sql;

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    rows.Add(new VirtualTourStopSequenceRowViewModel
                    {
                        ExhibitionId = reader.GetInt32(0),
                        ExhibitionTitle = reader.GetString(1),
                        PlanId = reader.GetInt32(2),
                        PlanTitle = reader.GetString(3),
                        StepNo = reader.GetInt32(4),
                        RoomId = reader.GetInt32(5),
                        RoomName = reader.GetString(6),
                        StopTitle = reader.GetString(7),
                        Notes = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                        EstimatedMinutes = reader.IsDBNull(9) ? null : reader.GetInt32(9)
                    });
                }
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    await connection.CloseAsync();
            }

            return View(rows);
        }


        public async Task<IActionResult> ExportVirtualTourStopSequenceHtml()
        {
            var rows = new List<VirtualTourStopSequenceRowViewModel>();

            var sql = @"
SELECT
    E.Exhibition_ID,
    E.Title AS Exhibition_Title,
    P.Plan_ID,
    P.Title AS Plan_Title,
    S.Step_No,
    S.Room_ID,
    S.Room_Name,
    S.Stop_Title,
    S.Notes,
    S.Estimated_Minutes
FROM VIRTUAL_TOUR_PLAN P
INNER JOIN EXHIBITION E
    ON P.Exhibition_ID = E.Exhibition_ID
INNER JOIN VIRTUAL_TOUR_STOP S
    ON P.Plan_ID = S.Plan_ID
ORDER BY
    P.Plan_ID,
    S.Step_No;";

            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = sql;

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    rows.Add(new VirtualTourStopSequenceRowViewModel
                    {
                        ExhibitionId = reader.GetInt32(0),
                        ExhibitionTitle = reader.GetString(1),
                        PlanId = reader.GetInt32(2),
                        PlanTitle = reader.GetString(3),
                        StepNo = reader.GetInt32(4),
                        RoomId = reader.GetInt32(5),
                        RoomName = reader.GetString(6),
                        StopTitle = reader.GetString(7),
                        Notes = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                        EstimatedMinutes = reader.IsDBNull(9) ? null : reader.GetInt32(9)
                    });
                }
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    await connection.CloseAsync();
            }

            string EscapeHtml(string? value)
            {
                if (string.IsNullOrEmpty(value))
                    return string.Empty;

                return System.Net.WebUtility.HtmlEncode(value);
            }

            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\" />");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
            sb.AppendLine("<title>Virtual Tour Stop Sequence</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 24px; }");
            sb.AppendLine("h1 { margin-bottom: 8px; }");
            sb.AppendLine("p { color: #555; margin-bottom: 20px; }");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; }");
            sb.AppendLine("th, td { border: 1px solid #ccc; padding: 8px; text-align: left; vertical-align: top; }");
            sb.AppendLine("th { background-color: #f3f3f3; }");
            sb.AppendLine("tr:nth-child(even) { background-color: #fafafa; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<h1>Virtual Tour Stop Sequence</h1>");
            sb.AppendLine("<p>This report displays the ordered stop sequence of each virtual tour plan together with exhibition and room information.</p>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Exhibition ID</th>");
            sb.AppendLine("<th>Exhibition Title</th>");
            sb.AppendLine("<th>Plan ID</th>");
            sb.AppendLine("<th>Plan Title</th>");
            sb.AppendLine("<th>Step No</th>");
            sb.AppendLine("<th>Room ID</th>");
            sb.AppendLine("<th>Room Name</th>");
            sb.AppendLine("<th>Stop Title</th>");
            sb.AppendLine("<th>Notes</th>");
            sb.AppendLine("<th>Estimated Minutes</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            foreach (var row in rows)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{row.ExhibitionId}</td>");
                sb.AppendLine($"<td>{EscapeHtml(row.ExhibitionTitle)}</td>");
                sb.AppendLine($"<td>{row.PlanId}</td>");
                sb.AppendLine($"<td>{EscapeHtml(row.PlanTitle)}</td>");
                sb.AppendLine($"<td>{row.StepNo}</td>");
                sb.AppendLine($"<td>{row.RoomId}</td>");
                sb.AppendLine($"<td>{EscapeHtml(row.RoomName)}</td>");
                sb.AppendLine($"<td>{EscapeHtml(row.StopTitle)}</td>");
                sb.AppendLine($"<td>{EscapeHtml(row.Notes)}</td>");
                sb.AppendLine($"<td>{(row.EstimatedMinutes.HasValue ? row.EstimatedMinutes.Value.ToString() : "-")}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            return File(bytes, "text/html", "VirtualTourStopSequence.html");
        }

        public async Task<IActionResult> EraRepresentationInActiveExhibitions()
        {
            var rows = new List<EraRepresentationInActiveExhibitionsRowViewModel>();

            var sql = @"
SELECT
    M.Era,
    COUNT(DISTINCT AW.Artwork_ID) AS Artwork_Count
FROM EXHIBITION E
INNER JOIN FEATURES F
    ON E.Exhibition_ID = F.Exhibition_ID
INNER JOIN ARTWORK AW
    ON F.Artwork_ID = AW.Artwork_ID
INNER JOIN BELONGS_TO B
    ON AW.Artwork_ID = B.Artwork_ID
INNER JOIN ART_MOVEMENT M
    ON B.Movement_ID = M.Movement_ID
WHERE E.Is_Active = 1
  AND M.Era IS NOT NULL
GROUP BY
    M.Era
ORDER BY
    Artwork_Count DESC,
    M.Era ASC;";

            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = sql;

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    rows.Add(new EraRepresentationInActiveExhibitionsRowViewModel
                    {
                        Era = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                        ArtworkCount = reader.GetInt32(1)
                    });
                }
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    await connection.CloseAsync();
            }

            return View(rows);
        }
        public async Task<IActionResult> ExportEraRepresentationInActiveExhibitionsHtml()
        {
            var rows = new List<EraRepresentationInActiveExhibitionsRowViewModel>();

            var sql = @"
SELECT
    M.Era,
    COUNT(DISTINCT AW.Artwork_ID) AS Artwork_Count
FROM EXHIBITION E
INNER JOIN FEATURES F
    ON E.Exhibition_ID = F.Exhibition_ID
INNER JOIN ARTWORK AW
    ON F.Artwork_ID = AW.Artwork_ID
INNER JOIN BELONGS_TO B
    ON AW.Artwork_ID = B.Artwork_ID
INNER JOIN ART_MOVEMENT M
    ON B.Movement_ID = M.Movement_ID
WHERE E.Is_Active = 1
  AND M.Era IS NOT NULL
GROUP BY
    M.Era
ORDER BY
    Artwork_Count DESC,
    M.Era ASC;";

            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = sql;

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    rows.Add(new EraRepresentationInActiveExhibitionsRowViewModel
                    {
                        Era = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                        ArtworkCount = reader.GetInt32(1)
                    });
                }
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    await connection.CloseAsync();
            }

            string EscapeHtml(string? value)
            {
                if (string.IsNullOrEmpty(value))
                    return string.Empty;

                return System.Net.WebUtility.HtmlEncode(value);
            }

            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\" />");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
            sb.AppendLine("<title>Era Representation in Active Exhibitions</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 24px; }");
            sb.AppendLine("h1 { margin-bottom: 8px; }");
            sb.AppendLine("p { color: #555; margin-bottom: 20px; }");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; }");
            sb.AppendLine("th, td { border: 1px solid #ccc; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background-color: #f3f3f3; }");
            sb.AppendLine("tr:nth-child(even) { background-color: #fafafa; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<h1>Era Representation in Active Exhibitions</h1>");
            sb.AppendLine("<p>This report shows which historical eras are represented in active exhibitions and how many artworks belong to each era.</p>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Era</th>");
            sb.AppendLine("<th>Artwork Count</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            foreach (var row in rows)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{EscapeHtml(row.Era)}</td>");
                sb.AppendLine($"<td>{row.ArtworkCount}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            return File(bytes, "text/html", "EraRepresentationInActiveExhibitions.html");
        }

        public async Task<IActionResult> MostPopularTours()
        {
            var rows = new List<MostPopularToursRowViewModel>();

            var sql = @"
SELECT
    T.Tour_ID,
    T.Title AS Tour_Title,
    E.Title AS Exhibition_Title,
    T.Tour_Date,
    T.Capacity,
    COUNT(TB.Booking_ID) AS Total_Bookings,
    SUM(CASE WHEN TB.Status = 'Booked' THEN 1 ELSE 0 END) AS Active_Bookings,
    SUM(CASE WHEN TB.Status = 'Cancelled' THEN 1 ELSE 0 END) AS Cancelled_Bookings,
    T.Capacity - SUM(CASE WHEN TB.Status = 'Booked' THEN 1 ELSE 0 END) AS Remaining_Capacity
FROM TOUR T
INNER JOIN EXHIBITION E
    ON T.Exhibition_ID = E.Exhibition_ID
LEFT JOIN TOUR_BOOKING TB
    ON T.Tour_ID = TB.Tour_ID
GROUP BY
    T.Tour_ID,
    T.Title,
    E.Title,
    T.Tour_Date,
    T.Capacity
ORDER BY
    Active_Bookings DESC,
    Total_Bookings DESC,
    T.Tour_Date ASC,
    T.Title ASC;";

            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = sql;

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    rows.Add(new MostPopularToursRowViewModel
                    {
                        TourId = reader.GetInt32(0),
                        TourTitle = reader.GetString(1),
                        ExhibitionTitle = reader.GetString(2),
                        TourDate = reader.GetDateTime(3),
                        Capacity = reader.GetInt32(4),
                        TotalBookings = reader.GetInt32(5),
                        ActiveBookings = reader.GetInt32(6),
                        CancelledBookings = reader.GetInt32(7),
                        RemainingCapacity = reader.GetInt32(8)
                    });
                }
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    await connection.CloseAsync();
            }

            return View(rows);
        }
        public async Task<IActionResult> ExportMostPopularToursHtml()
        {
            var rows = new List<MostPopularToursRowViewModel>();

            var sql = @"
SELECT
    T.Tour_ID,
    T.Title AS Tour_Title,
    E.Title AS Exhibition_Title,
    T.Tour_Date,
    T.Capacity,
    COUNT(TB.Booking_ID) AS Total_Bookings,
    SUM(CASE WHEN TB.Status = 'Booked' THEN 1 ELSE 0 END) AS Active_Bookings,
    SUM(CASE WHEN TB.Status = 'Cancelled' THEN 1 ELSE 0 END) AS Cancelled_Bookings,
    T.Capacity - SUM(CASE WHEN TB.Status = 'Booked' THEN 1 ELSE 0 END) AS Remaining_Capacity
FROM TOUR T
INNER JOIN EXHIBITION E
    ON T.Exhibition_ID = E.Exhibition_ID
LEFT JOIN TOUR_BOOKING TB
    ON T.Tour_ID = TB.Tour_ID
GROUP BY
    T.Tour_ID,
    T.Title,
    E.Title,
    T.Tour_Date,
    T.Capacity
ORDER BY
    Active_Bookings DESC,
    Total_Bookings DESC,
    T.Tour_Date ASC,
    T.Title ASC;";

            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = sql;

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    rows.Add(new MostPopularToursRowViewModel
                    {
                        TourId = reader.GetInt32(0),
                        TourTitle = reader.GetString(1),
                        ExhibitionTitle = reader.GetString(2),
                        TourDate = reader.GetDateTime(3),
                        Capacity = reader.GetInt32(4),
                        TotalBookings = reader.GetInt32(5),
                        ActiveBookings = reader.GetInt32(6),
                        CancelledBookings = reader.GetInt32(7),
                        RemainingCapacity = reader.GetInt32(8)
                    });
                }
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    await connection.CloseAsync();
            }

            string EscapeHtml(string? value)
            {
                if (string.IsNullOrEmpty(value))
                    return string.Empty;

                return System.Net.WebUtility.HtmlEncode(value);
            }

            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\" />");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
            sb.AppendLine("<title>Most Popular Tours</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 24px; }");
            sb.AppendLine("h1 { margin-bottom: 8px; }");
            sb.AppendLine("p { color: #555; margin-bottom: 20px; }");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; }");
            sb.AppendLine("th, td { border: 1px solid #ccc; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background-color: #f3f3f3; }");
            sb.AppendLine("tr:nth-child(even) { background-color: #fafafa; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<h1>Most Popular Tours</h1>");
            sb.AppendLine("<p>This report shows guided tours together with booking statistics, including active bookings, cancelled bookings, and remaining capacity.</p>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Tour ID</th>");
            sb.AppendLine("<th>Tour Title</th>");
            sb.AppendLine("<th>Exhibition Title</th>");
            sb.AppendLine("<th>Tour Date</th>");
            sb.AppendLine("<th>Capacity</th>");
            sb.AppendLine("<th>Total Bookings</th>");
            sb.AppendLine("<th>Active Bookings</th>");
            sb.AppendLine("<th>Cancelled Bookings</th>");
            sb.AppendLine("<th>Remaining Capacity</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            foreach (var row in rows)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{row.TourId}</td>");
                sb.AppendLine($"<td>{EscapeHtml(row.TourTitle)}</td>");
                sb.AppendLine($"<td>{EscapeHtml(row.ExhibitionTitle)}</td>");
                sb.AppendLine($"<td>{row.TourDate:yyyy-MM-dd}</td>");
                sb.AppendLine($"<td>{row.Capacity}</td>");
                sb.AppendLine($"<td>{row.TotalBookings}</td>");
                sb.AppendLine($"<td>{row.ActiveBookings}</td>");
                sb.AppendLine($"<td>{row.CancelledBookings}</td>");
                sb.AppendLine($"<td>{row.RemainingCapacity}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            return File(bytes, "text/html", "MostPopularTours.html");
        }   

        public async Task<IActionResult> MostVisitedExhibitions()
        {
            var rows = new List<MostVisitedExhibitionsRowViewModel>();

            var sql = @"
SELECT
    E.Exhibition_ID,
    E.Title AS Exhibition_Title,
    COUNT(V.User_ID) AS Total_Visits,
    SUM(CASE WHEN V.Status = 'Planned' THEN 1 ELSE 0 END) AS Planned_Visits,
    SUM(CASE WHEN V.Status = 'Completed' THEN 1 ELSE 0 END) AS Completed_Visits,
    SUM(CASE WHEN V.Status = 'Cancelled' THEN 1 ELSE 0 END) AS Cancelled_Visits
FROM EXHIBITION E
LEFT JOIN VISIT V
    ON E.Exhibition_ID = V.Exhibition_ID
GROUP BY
    E.Exhibition_ID,
    E.Title
ORDER BY
    Total_Visits DESC,
    Completed_Visits DESC,
    Planned_Visits DESC,
    E.Title ASC;";

            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = sql;

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    rows.Add(new MostVisitedExhibitionsRowViewModel
                    {
                        ExhibitionId = reader.GetInt32(0),
                        ExhibitionTitle = reader.GetString(1),
                        TotalVisits = reader.GetInt32(2),
                        PlannedVisits = reader.GetInt32(3),
                        CompletedVisits = reader.GetInt32(4),
                        CancelledVisits = reader.GetInt32(5)
                    });
                }
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    await connection.CloseAsync();
            }

            return View(rows);
        }
        public async Task<IActionResult> ExportMostVisitedExhibitionsHtml()
        {
            var rows = new List<MostVisitedExhibitionsRowViewModel>();

            var sql = @"
SELECT
    E.Exhibition_ID,
    E.Title AS Exhibition_Title,
    COUNT(V.User_ID) AS Total_Visits,
    SUM(CASE WHEN V.Status = 'Planned' THEN 1 ELSE 0 END) AS Planned_Visits,
    SUM(CASE WHEN V.Status = 'Completed' THEN 1 ELSE 0 END) AS Completed_Visits,
    SUM(CASE WHEN V.Status = 'Cancelled' THEN 1 ELSE 0 END) AS Cancelled_Visits
FROM EXHIBITION E
LEFT JOIN VISIT V
    ON E.Exhibition_ID = V.Exhibition_ID
GROUP BY
    E.Exhibition_ID,
    E.Title
ORDER BY
    Total_Visits DESC,
    Completed_Visits DESC,
    Planned_Visits DESC,
    E.Title ASC;";

            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = sql;

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    rows.Add(new MostVisitedExhibitionsRowViewModel
                    {
                        ExhibitionId = reader.GetInt32(0),
                        ExhibitionTitle = reader.GetString(1),
                        TotalVisits = reader.GetInt32(2),
                        PlannedVisits = reader.GetInt32(3),
                        CompletedVisits = reader.GetInt32(4),
                        CancelledVisits = reader.GetInt32(5)
                    });
                }
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    await connection.CloseAsync();
            }

            string EscapeHtml(string? value)
            {
                if (string.IsNullOrEmpty(value))
                    return string.Empty;

                return System.Net.WebUtility.HtmlEncode(value);
            }

            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\" />");
            sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
            sb.AppendLine("<title>Most Visited Exhibitions</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 24px; }");
            sb.AppendLine("h1 { margin-bottom: 8px; }");
            sb.AppendLine("p { color: #555; margin-bottom: 20px; }");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; }");
            sb.AppendLine("th, td { border: 1px solid #ccc; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background-color: #f3f3f3; }");
            sb.AppendLine("tr:nth-child(even) { background-color: #fafafa; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<h1>Most Visited Exhibitions</h1>");
            sb.AppendLine("<p>This report shows exhibitions together with visit statistics and visit status distribution.</p>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Exhibition ID</th>");
            sb.AppendLine("<th>Exhibition Title</th>");
            sb.AppendLine("<th>Total Visits</th>");
            sb.AppendLine("<th>Planned Visits</th>");
            sb.AppendLine("<th>Completed Visits</th>");
            sb.AppendLine("<th>Cancelled Visits</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            foreach (var row in rows)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{row.ExhibitionId}</td>");
                sb.AppendLine($"<td>{EscapeHtml(row.ExhibitionTitle)}</td>");
                sb.AppendLine($"<td>{row.TotalVisits}</td>");
                sb.AppendLine($"<td>{row.PlannedVisits}</td>");
                sb.AppendLine($"<td>{row.CompletedVisits}</td>");
                sb.AppendLine($"<td>{row.CancelledVisits}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            return File(bytes, "text/html", "MostVisitedExhibitions.html");
        }
    }
}
