import { useContext, useEffect, useMemo, useState } from "react";

import {
    Button,
    Chip,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    MenuItem,
    Paper,
    Stack,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TextField,
    Tooltip,
    Typography
} from "@mui/material";

import UserService from "../services/UserService";
import AuthContext from "../features/auth/AuthContext";
import { useNotification } from "../features/notifications/NotificationContext";

import type { User } from "../types/User";
import type { CreateUserRequest } from "../types/CreateUser";
import type { UpdateUserRequest } from "../types/UpdateUser";

// Fixed set of roles the Create/Edit form always offers, regardless
// of which roles currently exist among loaded users.
const ROLE_NAMES = ["Admin", "Owner", "Customer"] as const;
type RoleName = typeof ROLE_NAMES[number];

// Centralized fallback RoleIds, used only for a role that has no
// matching user yet in the currently loaded list (e.g. a fresh
// database with no Owner accounts). Whenever a role IS present in
// the loaded users, its real RoleId (from the API response) is used
// instead - this fallback is the only hardcoded mapping in the page.
const FALLBACK_ROLE_IDS: Record<RoleName, number> = {
    Admin: 1,
    Owner: 2,
    Customer: 3
};

function buildRoleIdMap(users: User[]): Record<RoleName, number> {

    const map = { ...FALLBACK_ROLE_IDS };

    for (const user of users) {

        if ((ROLE_NAMES as readonly string[]).includes(user.roleName)) {

            map[user.roleName as RoleName] = user.roleId;

        }

    }

    return map;

}

function getRoleChipColor(
    roleName: string
): "secondary" | "primary" | "default" {

    switch (roleName) {

        case "Admin":
            return "secondary";

        case "Owner":
            return "primary";

        default:
            return "default";

    }

}

interface UserFormState {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    phoneNumber: string;
    roleName: RoleName;
    isActive: boolean;
}

const EMPTY_FORM: UserFormState = {
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    phoneNumber: "",
    roleName: "Customer",
    isActive: true
};

function AdminUsersPage() {

    const { notify } = useNotification();

    const auth = useContext(AuthContext);

    if (!auth)
        throw new Error("AuthContext not found.");

    const currentUserId = auth.user?.userId ?? null;

    const [users, setUsers] = useState<User[]>([]);

    const [loading, setLoading] = useState(true);

    const [search, setSearch] = useState("");

    const [roleFilter, setRoleFilter] = useState("All");

    const [statusFilter, setStatusFilter] = useState("All");

    const [dialogOpen, setDialogOpen] = useState(false);

    const [editingUser, setEditingUser] = useState<User | null>(null);

    const [form, setForm] = useState<UserFormState>(EMPTY_FORM);

    const [deleteTargetId, setDeleteTargetId] = useState<number | null>(null);

    useEffect(() => {

        loadUsers();

    }, []);

    async function loadUsers() {

        try {

            const data = await UserService.getAll();

            setUsers(data);

        }
        catch (error: any) {

            console.error(error);

            notify(
                error.response?.data?.Message ??
                error.message ??
                "Failed to load users. Please try again.",
                "error"
            );

        }
        finally {

            setLoading(false);

        }

    }

    const roleIdMap = useMemo(
        () => buildRoleIdMap(users),
        [users]
    );

    const filteredUsers = useMemo(() => {

        const searchLower = search.trim().toLowerCase();

        return users.filter(user => {

            const matchesSearch =
                searchLower === "" ||
                `${user.firstName} ${user.lastName}`
                    .toLowerCase()
                    .includes(searchLower) ||
                user.email.toLowerCase().includes(searchLower);

            const matchesRole =
                roleFilter === "All" ||
                user.roleName === roleFilter;

            const matchesStatus =
                statusFilter === "All" ||
                (statusFilter === "Active" && user.isActive) ||
                (statusFilter === "Inactive" && !user.isActive);

            return matchesSearch && matchesRole && matchesStatus;

        });

    }, [users, search, roleFilter, statusFilter]);

    function openCreateDialog() {

        setEditingUser(null);

        setForm(EMPTY_FORM);

        setDialogOpen(true);

    }

    function openEditDialog(user: User) {

        setEditingUser(user);

        setForm({
            firstName: user.firstName,
            lastName: user.lastName,
            email: user.email,
            password: "",
            phoneNumber: user.phoneNumber,
            roleName: (ROLE_NAMES as readonly string[]).includes(user.roleName)
                ? (user.roleName as RoleName)
                : "Customer",
            isActive: user.isActive
        });

        setDialogOpen(true);

    }

    function closeDialog() {

        setDialogOpen(false);

        setEditingUser(null);

        setForm(EMPTY_FORM);

    }

    async function saveUser() {

        try {

            if (editingUser === null) {

                const dto: CreateUserRequest = {
                    firstName: form.firstName,
                    lastName: form.lastName,
                    email: form.email,
                    password: form.password,
                    phoneNumber: form.phoneNumber,
                    roleId: roleIdMap[form.roleName],
                    isActive: form.isActive
                };

                await UserService.create(dto);

                notify("User created successfully.", "success");

            }
            else {

                const dto: UpdateUserRequest = {
                    firstName: form.firstName,
                    lastName: form.lastName,
                    email: form.email,
                    phoneNumber: form.phoneNumber,
                    roleId: roleIdMap[form.roleName],
                    isActive: form.isActive
                };

                await UserService.update(editingUser.id, dto);

                notify("User updated successfully.", "success");

            }

            closeDialog();

            await loadUsers();

        }
        catch (error: any) {

            console.error(error);

            notify(
                error.response?.data?.Message ??
                error.message ??
                "Something went wrong. Please try again.",
                "error"
            );

        }

    }

    async function toggleActive(user: User) {

        try {

            const dto: UpdateUserRequest = {
                firstName: user.firstName,
                lastName: user.lastName,
                email: user.email,
                phoneNumber: user.phoneNumber,
                roleId: user.roleId,
                isActive: !user.isActive
            };

            await UserService.update(user.id, dto);

            notify(
                user.isActive
                    ? "User deactivated."
                    : "User activated.",
                "success"
            );

            await loadUsers();

        }
        catch (error: any) {

            console.error(error);

            notify(
                error.response?.data?.Message ??
                error.message ??
                "Something went wrong. Please try again.",
                "error"
            );

        }

    }

    function closeDeleteDialog() {

        setDeleteTargetId(null);

    }

    async function confirmDeleteUser() {

        if (deleteTargetId === null)
            return;

        try {

            await UserService.delete(deleteTargetId);

            notify("User deleted successfully.", "success");

            closeDeleteDialog();

            await loadUsers();

        }
        catch (error: any) {

            console.error(error);

            notify(
                error.response?.data?.Message ??
                error.message ??
                "Something went wrong. Please try again.",
                "error"
            );

            closeDeleteDialog();

        }

    }

    const isEditingSelf =
        editingUser !== null && editingUser.id === currentUserId;

    if (loading)
        return <CircularProgress />;

    return (

        <>

            <Stack
                direction="row"
                spacing={2}
                sx={{
                    justifyContent: "space-between",
                    alignItems: "center",
                    mb: 3
                }}
            >

                <Typography variant="h4">
                    User Management
                </Typography>

                <Button
                    variant="contained"
                    onClick={openCreateDialog}
                >
                    Create User
                </Button>

            </Stack>

            <Stack
                direction="row"
                spacing={2}
                sx={{ mb: 3 }}
            >

                <TextField
                    label="Search users..."
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    sx={{ minWidth: 250 }}
                />

                <TextField
                    select
                    label="Role"
                    value={roleFilter}
                    onChange={(e) => setRoleFilter(e.target.value)}
                    sx={{ minWidth: 180 }}
                >

                    <MenuItem value="All">All Roles</MenuItem>

                    {ROLE_NAMES.map(roleName => (

                        <MenuItem
                            key={roleName}
                            value={roleName}
                        >
                            {roleName}
                        </MenuItem>

                    ))}

                </TextField>

                <TextField
                    select
                    label="Status"
                    value={statusFilter}
                    onChange={(e) => setStatusFilter(e.target.value)}
                    sx={{ minWidth: 180 }}
                >

                    <MenuItem value="All">All Statuses</MenuItem>
                    <MenuItem value="Active">Active</MenuItem>
                    <MenuItem value="Inactive">Inactive</MenuItem>

                </TextField>

            </Stack>

            {filteredUsers.length === 0 ? (

                <Typography color="text.secondary">
                    No users match the current filters.
                </Typography>

            ) : (

                <TableContainer component={Paper}>

                    <Table>

                        <TableHead>

                            <TableRow>

                                <TableCell>Name</TableCell>
                                <TableCell>Email</TableCell>
                                <TableCell>Phone</TableCell>
                                <TableCell>Role</TableCell>
                                <TableCell>Status</TableCell>
                                <TableCell>Actions</TableCell>

                            </TableRow>

                        </TableHead>

                        <TableBody>

                            {filteredUsers.map(user => {

                                const isSelf = user.id === currentUserId;

                                return (

                                    <TableRow key={user.id}>

                                        <TableCell>
                                            {user.firstName} {user.lastName}
                                        </TableCell>

                                        <TableCell>
                                            {user.email}
                                        </TableCell>

                                        <TableCell>
                                            {user.phoneNumber}
                                        </TableCell>

                                        <TableCell>

                                            <Chip
                                                label={user.roleName}
                                                color={getRoleChipColor(user.roleName)}
                                            />

                                        </TableCell>

                                        <TableCell>

                                            <Chip
                                                label={user.isActive ? "Active" : "Inactive"}
                                                color={user.isActive ? "success" : "default"}
                                            />

                                        </TableCell>

                                        <TableCell>

                                            <Stack
                                                direction="row"
                                                spacing={1}
                                                flexWrap="wrap"
                                            >

                                                <Button
                                                    size="small"
                                                    variant="outlined"
                                                    onClick={() => openEditDialog(user)}
                                                >
                                                    Edit
                                                </Button>

                                                {isSelf ? (

                                                    <Tooltip title="You cannot deactivate your own account.">

                                                        <span>

                                                            <Button
                                                                size="small"
                                                                variant="outlined"
                                                                disabled
                                                            >
                                                                Deactivate
                                                            </Button>

                                                        </span>

                                                    </Tooltip>

                                                ) : (

                                                    <Button
                                                        size="small"
                                                        variant="outlined"
                                                        color={user.isActive ? "warning" : "success"}
                                                        onClick={() => toggleActive(user)}
                                                    >
                                                        {user.isActive ? "Deactivate" : "Activate"}
                                                    </Button>

                                                )}

                                                {isSelf ? (

                                                    <Tooltip title="You cannot delete your own account.">

                                                        <span>

                                                            <Button
                                                                size="small"
                                                                variant="outlined"
                                                                color="error"
                                                                disabled
                                                            >
                                                                Delete
                                                            </Button>

                                                        </span>

                                                    </Tooltip>

                                                ) : (

                                                    <Button
                                                        size="small"
                                                        variant="outlined"
                                                        color="error"
                                                        onClick={() => setDeleteTargetId(user.id)}
                                                    >
                                                        Delete
                                                    </Button>

                                                )}

                                            </Stack>

                                        </TableCell>

                                    </TableRow>

                                );

                            })}

                        </TableBody>

                    </Table>

                </TableContainer>

            )}

            <Dialog
                open={dialogOpen}
                onClose={closeDialog}
            >

                <DialogTitle>
                    {editingUser === null ? "Create User" : "Edit User"}
                </DialogTitle>

                <DialogContent>

                    <Stack spacing={2} sx={{ mt: 1, minWidth: 350 }}>

                        <TextField
                            label="First Name"
                            value={form.firstName}
                            onChange={(e) =>
                                setForm({ ...form, firstName: e.target.value })
                            }
                        />

                        <TextField
                            label="Last Name"
                            value={form.lastName}
                            onChange={(e) =>
                                setForm({ ...form, lastName: e.target.value })
                            }
                        />

                        <TextField
                            label="Email"
                            value={form.email}
                            onChange={(e) =>
                                setForm({ ...form, email: e.target.value })
                            }
                        />

                        {editingUser === null && (

                            <TextField
                                label="Password"
                                type="password"
                                value={form.password}
                                onChange={(e) =>
                                    setForm({ ...form, password: e.target.value })
                                }
                            />

                        )}

                        <TextField
                            label="Phone Number"
                            value={form.phoneNumber}
                            onChange={(e) =>
                                setForm({ ...form, phoneNumber: e.target.value })
                            }
                        />

                        <Tooltip
                            title={
                                isEditingSelf
                                    ? "You cannot change your own role away from Admin."
                                    : ""
                            }
                        >

                            <TextField
                                select
                                label="Role"
                                value={form.roleName}
                                disabled={isEditingSelf}
                                onChange={(e) =>
                                    setForm({
                                        ...form,
                                        roleName: e.target.value as RoleName
                                    })
                                }
                            >

                                {ROLE_NAMES.map(roleName => (

                                    <MenuItem
                                        key={roleName}
                                        value={roleName}
                                    >
                                        {roleName}
                                    </MenuItem>

                                ))}

                            </TextField>

                        </Tooltip>

                        <Tooltip
                            title={
                                isEditingSelf
                                    ? "You cannot deactivate your own account."
                                    : ""
                            }
                        >

                            <TextField
                                select
                                label="Active"
                                value={form.isActive ? "Active" : "Inactive"}
                                disabled={isEditingSelf}
                                onChange={(e) =>
                                    setForm({
                                        ...form,
                                        isActive: e.target.value === "Active"
                                    })
                                }
                            >

                                <MenuItem value="Active">Active</MenuItem>
                                <MenuItem value="Inactive">Inactive</MenuItem>

                            </TextField>

                        </Tooltip>

                    </Stack>

                </DialogContent>

                <DialogActions>

                    <Button onClick={closeDialog}>
                        Cancel
                    </Button>

                    <Button
                        variant="contained"
                        onClick={saveUser}
                    >
                        {editingUser === null ? "Create" : "Save"}
                    </Button>

                </DialogActions>

            </Dialog>

            <Dialog
                open={deleteTargetId !== null}
                onClose={closeDeleteDialog}
            >

                <DialogTitle>
                    Delete User
                </DialogTitle>

                <DialogContent>

                    <Typography>
                        Are you sure you want to delete this user? This cannot be undone.
                    </Typography>

                </DialogContent>

                <DialogActions>

                    <Button onClick={closeDeleteDialog}>
                        Cancel
                    </Button>

                    <Button
                        variant="contained"
                        color="error"
                        onClick={confirmDeleteUser}
                    >
                        Delete
                    </Button>

                </DialogActions>

            </Dialog>

        </>

    );

}

export default AdminUsersPage;
