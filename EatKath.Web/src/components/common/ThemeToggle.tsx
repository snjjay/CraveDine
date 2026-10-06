import { IconButton, Tooltip } from "@mui/material";
import { useColorScheme } from "@mui/material/styles";

import DarkModeOutlinedIcon from "@mui/icons-material/DarkModeOutlined";
import LightModeOutlinedIcon from "@mui/icons-material/LightModeOutlined";

// Day/Night switch for the header and the mobile menu. Shows the current
// theme (sun = Day, moon = Night); the label says what a click does.
// Until the user picks, the theme follows the OS ("system"); the choice
// is then saved by MUI in localStorage.
function ThemeToggle() {

    const { mode, systemMode, setMode } = useColorScheme();

    const isNight = (mode === "system" ? systemMode : mode) === "dark";

    const label = isNight ? "Switch to day theme" : "Switch to night theme";

    return (

        <Tooltip title={isNight ? "Switch to Day" : "Switch to Night"}>
            <IconButton
                data-theme-toggle
                aria-label={label}
                onClick={() => setMode(isNight ? "light" : "dark")}
                sx={{
                    width: 44,
                    height: 44,
                    color: "text.secondary",
                    "&:hover": { color: "text.primary" }
                }}
            >
                {isNight
                    ? <DarkModeOutlinedIcon />
                    : <LightModeOutlinedIcon />}
            </IconButton>
        </Tooltip>

    );

}

export default ThemeToggle;
