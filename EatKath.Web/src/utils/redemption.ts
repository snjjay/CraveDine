// ==========================================================
// Redemption status
// ==========================================================
//
// The API sends RedemptionStatus as its numeric enum value.
// Matches: EatKath.API.Enums.RedemptionStatus
// ==========================================================

export const RedemptionStatus = {
    Redeemed: 1,
    Completed: 2,
    Cancelled: 3,
    Expired: 4
} as const;

type ChipColor = "success" | "warning" | "error" | "info" | "default";

export function getRedemptionStatusLabel(status: number): string {

    switch (status) {

        case RedemptionStatus.Redeemed:
            return "Redeemed";

        case RedemptionStatus.Completed:
            return "Completed";

        case RedemptionStatus.Cancelled:
            return "Cancelled";

        case RedemptionStatus.Expired:
            return "Expired";

        default:
            return "Unknown";

    }

}

export function getRedemptionStatusColor(status: number): ChipColor {

    switch (status) {

        case RedemptionStatus.Redeemed:
            return "warning";

        case RedemptionStatus.Completed:
            return "success";

        case RedemptionStatus.Expired:
            return "error";

        default:
            return "default";

    }

}
