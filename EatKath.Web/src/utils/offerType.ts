// Matches: EatKath.API.Enums.OfferType (sent as its numeric value)
export function getOfferTypeLabel(offerType: number): string {

    switch (offerType) {

        case 1:
            return "Dine In";

        case 2:
            return "Takeaway";

        case 3:
            return "Delivery";

        default:
            return "Offer";

    }

}
