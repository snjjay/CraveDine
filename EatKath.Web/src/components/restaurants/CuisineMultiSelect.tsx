// CuisineMultiSelect = the "Cuisines" field on the owner Create and Edit
// Restaurant forms: pick one or more existing cuisines, shown as chips.

import {
    Box,
    Checkbox,
    Chip,
    ListItemText,
    MenuItem,
    TextField
} from "@mui/material";

import type { Cuisine } from "../../types/Cuisine";

interface Props {
    cuisines: Cuisine[];
    // Selected Cuisine ids.
    value: number[];
    onChange: (cuisineIds: number[]) => void;
    disabled?: boolean;
}

function CuisineMultiSelect({ cuisines, value, onChange, disabled }: Props) {

    const nameById = new Map(cuisines.map(c => [c.id, c.name]));

    return (

        <TextField
            select
            label="Cuisines"
            required
            disabled={disabled}
            value={value}
            helperText="Select one or more cuisines."
            onChange={(e) => {

                // A multiple select reports number[] (or a comma
                // string when autofilled).
                const selected = e.target.value as unknown as number[] | string;

                onChange(
                    (typeof selected === "string" ? selected.split(",") : selected)
                        .map(Number)
                );

            }}
            slotProps={{
                select: {
                    multiple: true,
                    renderValue: (selected) => (
                        <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
                            {(selected as number[]).map(id => (
                                <Chip
                                    key={id}
                                    size="small"
                                    label={nameById.get(id) ?? `Cuisine #${id}`}
                                />
                            ))}
                        </Box>
                    )
                }
            }}
        >

            {cuisines.map(cuisine => (

                <MenuItem key={cuisine.id} value={cuisine.id}>
                    <Checkbox
                        size="small"
                        checked={value.includes(cuisine.id)}
                        sx={{ p: 0, mr: 1.5 }}
                        tabIndex={-1}
                    />
                    <ListItemText primary={cuisine.name} />
                </MenuItem>

            ))}

        </TextField>

    );

}

export default CuisineMultiSelect;
