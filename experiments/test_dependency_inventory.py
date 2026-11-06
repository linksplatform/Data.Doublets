"""Keep unsupported registry inputs explicit instead of failing accidentally."""

import importlib.util
from pathlib import Path
import unittest

spec = importlib.util.spec_from_file_location(
    "dependency_inventory", Path(__file__).parent / "issue-556/inventory.py"
)
inventory = importlib.util.module_from_spec(spec)
spec.loader.exec_module(inventory)


class InventoryTests(unittest.TestCase):
    def test_unsupported_ecosystem_retains_an_actionable_diagnostic(self):
        result = inventory.resolve(("unsupported", "example", "1.0"))
        self.assertEqual(result["ecosystem"], "unsupported")
        self.assertEqual(result["name"], "example")
        self.assertEqual(result["old"], "1.0")
        self.assertEqual(result["error"], "Unsupported ecosystem: unsupported")


if __name__ == "__main__":
    unittest.main()
