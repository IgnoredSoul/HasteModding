import os, shutil
def copy_dir(src: str, dst: str, exclude: set[str] = None) -> None:
    try:
        exclude = set(exclude or [])
        
        def ignore(path: str, names: str) -> list[str]:
            return [name for name in names if os.path.relpath(os.path.join(path, name), src) in exclude or name in exclude]

        print(f"Copying: {os.path.relpath(src)}")
        shutil.copytree(src, dst, ignore=ignore)
    except Exception as ex:
        print(f"Failed to copy: {src};\n{ex}")
        
def clear_dir(path: str, keep: set[str]) -> None:
    keep = set(keep or [])
    [shutil.rmtree(os.path.join(path, item), True) for item in os.listdir(path) if item not in keep]

clear_dir("./", keep={"UpdateList.py", ".git", '.gitignore', '.gitmodules', 'README.md'})
copy_dir(f'../HasteModding/SettingsLib', './SettingsLib', {'bin', '.git', 'obj', '.vs'})
copy_dir(f'../HasteModding/Informer', './Informer', {'bin', '.git', 'obj', '.vs'})
os.system("git submodule update --init --remote --recursive")
if str(input("Push? ")).lower() == "y": os.system("git add * && git commit -m \"New Updates\" && git push")
print("Finished.")