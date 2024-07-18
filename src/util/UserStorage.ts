
/**
 * Collection of utilities for interacting with local storage
 */
export default class UserStorageUtil {

    private static storageAvailable: boolean | undefined = undefined;

    /**
     * Check if the user storage is available for usage
     * 
     * @returns If the local storage can be used
     */
    public static available(): boolean {
        if (UserStorageUtil.storageAvailable == undefined) {
            try {
                const storage = localStorage;
                const x: string = '$__storage_test__';
                storage.setItem(x, x);
                storage.removeItem(x);
                UserStorageUtil.storageAvailable = true;
            } catch (e) {
                UserStorageUtil.storageAvailable = false;
                console.error(`UserStorageUtil> localStorage not available: ${e}`);
            }
        }

        return UserStorageUtil.storageAvailable;
    }

    /**
     * Print all keys that are stored in the local storage
     */
    public static printKeys(): void {
        if (UserStorageUtil.available() == false) {
            console.error(`UserStorageUtil> localStorage not available, cannot printKeys`);
            return;
        }

        console.log(`UserStorageUtil> localStorage has ${localStorage.length} values stored:`);
        for (let i = 0; i < localStorage.length; ++i) {
            console.log(`\t${localStorage.key(i)}`);
        }
    }

    /**
     * Get the keys that are stored in user storage, optionally starting with a specific value
     * 
     * @param startsWith Value the key must start with, defaults to null, meaning ignore
     * 
     * @returns All keys in the user storage that start with the parameter passed if not null, else all keys
     */
    public static getKeys(startsWith: string | null = null): string[] {
        if (UserStorageUtil.available() == false) { return []; }

        const data: string[] = [];

        if (startsWith != null) {
            for (let i = 0; i < localStorage.length; ++i) {
                // Force is safe
                const iterKey: string = localStorage.key(i)!;
                if (iterKey.startsWith(startsWith)) {
                    data.push(iterKey);
                }
            }
        } else {
            for (let i = 0; i < localStorage.length; ++i) {
                // Force is safe
                data.push(localStorage.key(i)!);
            }
        }

        return data;
    }

    /**
     * Get the item stored in localStorage under the given key
     * 
     * @param key Key to get the value of 
     * 
     * @returns If localStorage is available and the key is present in localStorage, the JSON parsed object of the
     *          value is returned. If localStorage is not available, null is returned. If the key is not stored
     *          in localStorage, null is returned
     */
    public static get<T>(key: string): T | null {
        if (UserStorageUtil.available() == false) {
            return null;
        }

        key = key.toLowerCase();

        const item: string | null = localStorage.getItem(key);
        if (item == null) {
            return null;
        }

        const data: T = JSON.parse(item) as T;

        return data;
    }

    /**
     * Set the data stored in the user storage
     * 
     * @param key   Key to store data under
     * @param data  Data to be stored in the user storage
     */
    public static set<T>(key: string, data: T): void {
        if (UserStorageUtil.available() == false) { return; }

        key = key.toLowerCase();

        localStorage.setItem(key, JSON.stringify(data));
    }

    /**
     * Remove a user stored value. If the key is not found, no operation is performed
     * 
     * @param key Key of the value to remove from the user store
     */
    public static remove(key: string): void {
        if (UserStorageUtil.available() == false) { return; }

        console.log(`UserStorageUtil> removed '${key.toLowerCase()}' from localStorage`);

        localStorage.removeItem(key.toLowerCase());
    }

}
