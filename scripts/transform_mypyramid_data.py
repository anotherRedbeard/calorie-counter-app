from __future__ import annotations

import argparse
import json
import re
import urllib.request
import xml.etree.ElementTree as ET
import zipfile
from dataclasses import asdict, dataclass
from decimal import Decimal, InvalidOperation
from pathlib import Path

SOURCE_URL = (
    "https://inventory.data.gov/dataset/"
    "794cd3d7-4d28-4408-8f7d-84b820dbf7f2/resource/"
    "6b78ec0c-4980-4ad8-9cbd-2d6eb9eda8e7/download/myfoodapediadata.zip"
)
DEFAULT_ZIP_PATH = Path("data/raw/myfoodapediadata.zip")
DEFAULT_OUTPUT_PATH = Path("src/CalorieCounter.Api/Data/food-data.json")
FOOD_DISPLAY_XML = "Food_Display_Table.xml"


@dataclass(slots=True)
class FoodRecord:
    foodCode: str
    displayName: str
    searchName: str
    portionAmount: float
    portionDisplayName: str
    portionDescription: str
    calories: float


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Transform USDA MyPyramid food data into runtime JSON."
    )
    parser.add_argument(
        "--source",
        type=Path,
        default=DEFAULT_ZIP_PATH,
        help=f"Path to the downloaded USDA ZIP file (default: {DEFAULT_ZIP_PATH})",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=DEFAULT_OUTPUT_PATH,
        help=f"Output JSON path (default: {DEFAULT_OUTPUT_PATH})",
    )
    parser.add_argument(
        "--skip-download",
        action="store_true",
        help="Fail if the ZIP file is missing instead of downloading it.",
    )
    return parser.parse_args()


def ensure_source_file(source_path: Path, skip_download: bool) -> None:
    if source_path.exists():
        return

    if skip_download:
        raise FileNotFoundError(f"Missing source file: {source_path}")

    source_path.parent.mkdir(parents=True, exist_ok=True)
    with urllib.request.urlopen(SOURCE_URL) as response, source_path.open("wb") as target:
        target.write(response.read())


def normalize_text(value: str | None) -> str:
    if value is None:
        return ""

    return re.sub(r"\s+", " ", value).strip()


def to_decimal(value: str | None) -> Decimal:
    normalized = normalize_text(value)
    if not normalized:
        return Decimal("0")

    try:
        return Decimal(normalized)
    except InvalidOperation as exc:
        raise ValueError(f"Could not parse decimal value: {value!r}") from exc


def format_portion(portion_amount: Decimal, portion_name: str) -> str:
    amount_text = format(portion_amount.normalize(), "f").rstrip("0").rstrip(".")
    if not amount_text:
        amount_text = "0"

    if portion_name:
        return f"{amount_text} {portion_name}"

    return amount_text


def parse_food_records(source_path: Path) -> list[FoodRecord]:
    with zipfile.ZipFile(source_path) as archive:
        root = ET.fromstring(archive.read(FOOD_DISPLAY_XML))

    records: list[FoodRecord] = []
    for row in root:
        values = {child.tag: normalize_text(child.text) for child in row}

        display_name = values["Display_Name"]
        portion_name = values["Portion_Display_Name"]
        portion_amount = to_decimal(values["Portion_Amount"])
        calories = to_decimal(values["Calories"])

        records.append(
            FoodRecord(
                foodCode=values["Food_Code"],
                displayName=display_name,
                searchName=display_name.casefold(),
                portionAmount=float(portion_amount),
                portionDisplayName=portion_name,
                portionDescription=format_portion(portion_amount, portion_name),
                calories=float(calories),
            )
        )

    return sorted(records, key=lambda record: (record.searchName, record.foodCode, record.portionAmount))


def write_output(records: list[FoodRecord], output_path: Path) -> None:
    output_path.parent.mkdir(parents=True, exist_ok=True)
    payload = [asdict(record) for record in records]
    output_path.write_text(json.dumps(payload, indent=2), encoding="utf-8")
    output_path.write_text(output_path.read_text(encoding="utf-8") + "\n", encoding="utf-8")


def main() -> None:
    args = parse_args()
    ensure_source_file(args.source, args.skip_download)
    records = parse_food_records(args.source)
    write_output(records, args.output)
    print(f"Wrote {len(records)} food records to {args.output}")


if __name__ == "__main__":
    main()
