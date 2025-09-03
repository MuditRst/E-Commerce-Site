import { useEffect, useState } from "react";
import User from "../../Interface/User";
import { deleteUser, getUsers } from "../../api";

function UserListActions() {
    const [allUsers, setAllUsers] = useState<User[]>([]);

    useEffect(() => {
            getUsers().then(res => {setAllUsers(res.data)}).catch(console.error);
        }, []);

    const handleDeleteUser = async (userId: string) => {
        try{
            const response = await deleteUser(userId);
            console.log("User deleted successfully:", response.data);
            setAllUsers(allUsers.filter(user => user.id !== userId));
        } catch (error) {
            console.error("Error deleting user:", error);
        }
    };

    return (
        <div>
        <h2>User List Actions</h2>
            <ul>
                {allUsers.map((user) =>
                    user.role === "User" ? (
                        <li key={user.id}>
                            <span>
                                User ID: {user.id}, Username: {user.username}
                            </span>
                            <button onClick={() => handleDeleteUser(user.id.toString())}>
                                Delete
                            </button>
                        </li>
                    ) : null
                )}
            </ul>
        </div>
    );
}
export default UserListActions;