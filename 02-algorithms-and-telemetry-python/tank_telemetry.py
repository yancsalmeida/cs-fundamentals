"""
Laboratory 02: Continuous Tank Volume Balance & Telemetry Analysis (Python 3)

Models discrete-time mass/volume balance in a storage tank with safety threshold
classification, summary statistics, and automated self-tests.
"""

from dataclasses import dataclass
from typing import Iterable, List
import unittest


@dataclass(frozen=True)
class TelemetryStep:
    step: int
    delta_liters: float
    volume_liters: float
    status: str


@dataclass(frozen=True)
class TelemetryReport:
    initial_volume_liters: float
    final_volume_liters: float
    min_volume_liters: float
    max_volume_liters: float
    mean_volume_liters: float
    steps: List[TelemetryStep]


def classify_tank_level(
    volume_liters: float,
    low_alarm_liters: float = 100.0,
    high_alarm_liters: float = 900.0,
) -> str:
    """Classifies tank volume against low and high safety thresholds."""
    if volume_liters > high_alarm_liters:
        return "CRITICAL_HIGH"
    if volume_liters < low_alarm_liters:
        return "CRITICAL_LOW"
    return "NORMAL"


def simulate_tank_balance(
    initial_volume_liters: float,
    flow_deltas_liters: Iterable[float],
    low_alarm_liters: float = 100.0,
    high_alarm_liters: float = 900.0,
) -> TelemetryReport:
    """
    Executes a discrete mass/volume balance simulation across a sequence of flow variations.
    Stops early if a delta of 0.0 is encountered (end-of-shift sentinel).
    """
    if initial_volume_liters < 0.0:
        raise ValueError("Initial volume cannot be negative.")

    current_volume = float(initial_volume_liters)
    steps: List[TelemetryStep] = [
        TelemetryStep(
            step=0,
            delta_liters=0.0,
            volume_liters=current_volume,
            status=classify_tank_level(current_volume, low_alarm_liters, high_alarm_liters),
        )
    ]

    for idx, delta in enumerate(flow_deltas_liters, start=1):
        if delta == 0.0:
            break
        current_volume = max(0.0, current_volume + float(delta))
        steps.append(
            TelemetryStep(
                step=idx,
                delta_liters=float(delta),
                volume_liters=current_volume,
                status=classify_tank_level(current_volume, low_alarm_liters, high_alarm_liters),
            )
        )

    volumes = [s.volume_liters for s in steps]
    return TelemetryReport(
        initial_volume_liters=initial_volume_liters,
        final_volume_liters=current_volume,
        min_volume_liters=min(volumes),
        max_volume_liters=max(volumes),
        mean_volume_liters=sum(volumes) / len(volumes),
        steps=steps,
    )


class TankTelemetryTests(unittest.TestCase):
    def test_threshold_classification(self) -> None:
        self.assertEqual(classify_tank_level(50.0), "CRITICAL_LOW")
        self.assertEqual(classify_tank_level(500.0), "NORMAL")
        self.assertEqual(classify_tank_level(950.0), "CRITICAL_HIGH")

    def test_volume_balance_simulation_and_sentinel_stop(self) -> None:
        report = simulate_tank_balance(
            initial_volume_liters=500.0,
            flow_deltas_liters=[250.0, 200.0, -880.0, 0.0, 300.0],
        )
        # 500 -> 750 -> 950 (HIGH) -> 70 (LOW) -> stops at 0.0 before 300.0
        self.assertEqual(len(report.steps), 4)
        self.assertAlmostEqual(report.final_volume_liters, 70.0)
        self.assertAlmostEqual(report.max_volume_liters, 950.0)
        self.assertAlmostEqual(report.min_volume_liters, 70.0)
        self.assertEqual(report.steps[2].status, "CRITICAL_HIGH")
        self.assertEqual(report.steps[3].status, "CRITICAL_LOW")


if __name__ == "__main__":
    sample = simulate_tank_balance(500.0, [200.0, 250.0, -400.0, -480.0, 300.0])
    print("=== Tank Volume Balance Telemetry Report ===")
    for s in sample.steps:
        print(f"Step {s.step:02d} | Delta: {s.delta_liters:+7.1f} L | Volume: {s.volume_liters:6.1f} L | {s.status}")
    print(
        f"Summary -> Min: {sample.min_volume_liters:.1f} L | "
        f"Max: {sample.max_volume_liters:.1f} L | "
        f"Mean: {sample.mean_volume_liters:.1f} L\n"
    )
    unittest.main()
