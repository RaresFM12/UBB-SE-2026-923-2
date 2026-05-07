using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UBB_SE_2026_923_2.Migrations
{
    /// <inheritdoc />
    public partial class SeedItemsQuantityZero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
            table: "Items",
            columns: new[]
            {
                "Id",
                "Name",
                "Producer",
                "Price",
                "Category",
                "ImagePath",
                "NumberOfPills",
                "Quantity",
                "Label",
                "Description",
                "DiscountPercentage"
            },
            values: new object[,]
            {
                {
                    100,
                    "Ibuprofen",
                    "Terapia",
                    18.5f,
                    "Pain Relief",
                    "..\\..\\Assets\\placeholder.png",
                    20,
                    0,
                    "Anti-inflammatory",
                    "Used for pain and inflammation",
                    0f
                },

                {
                    101,
                    "Aspirin",
                    "Bayer",
                    15f,
                    "Pain Relief",
                    "..\\..\\Assets\\placeholder.png",
                    30,
                    0,
                    "Painkiller",
                    "Used for headaches and fever",
                    0f
                },

                {
                    102,
                    "Amoxicillin",
                    "Sandoz",
                    32f,
                    "Antibiotic",
                    "..\\..\\Assets\\placeholder.png",
                    16,
                    0,
                    "Antibiotic",
                    "Broad-spectrum antibiotic",
                    0f
                },

                {
                    103,
                    "Cetirizine",
                    "Zentiva",
                    22f,
                    "Allergy",
                    "..\\..\\Assets\\placeholder.png",
                    10,
                    0,
                    "Antihistamine",
                    "Used for allergies",
                    0f
                },

                {
                    104,
                    "Omeprazole",
                    "Krka",
                    27f,
                    "Digestive",
                    "..\\..\\Assets\\placeholder.png",
                    14,
                    0,
                    "Stomach protection",
                    "Reduces stomach acid",
                    0f
                }
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
            table: "Items",
            keyColumn: "Id",
            keyValues: new object[]
            {
                100, 101, 102, 103, 104
            });
        }
    }
}
