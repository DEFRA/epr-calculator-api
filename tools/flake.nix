{
  description = "R environment for build_org_extract_from_results_file.R and verify_results_csv.R";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs { inherit system; };
        rEnv = pkgs.rWrapper.override {
          packages = with pkgs.rPackages; [ gmp ];
        };
      in
      {
        devShells.default = pkgs.mkShell {
          buildInputs = [ rEnv ];
        };

        # `nix run .#build-org-extract -- <results.csv> <out.csv>` -- must be
        # run from inside tools/. build_org_extract_from_results_file.R locates
        # its own directory (to default-resolve ../v_extract_recent_pom_org_data.csv)
        # the same way the Python original does, which only works against the
        # real checkout -- so this deliberately runs the in-tree script by a
        # plain relative path (resolved against $PWD) rather than a
        # store-copied ${./file.R}, whose containing directory is a
        # /nix/store path with no sibling files at all.
        apps.build-org-extract = {
          type = "app";
          program = "${pkgs.writeShellScript "build-org-extract" ''
            exec ${rEnv}/bin/Rscript build_org_extract_from_results_file.R "$@"
          ''}";
        };

        # `nix run .#verify-results-csv -- <results.csv> [--verbose]` -- has
        # no such sibling-file dependency, so the store-copied script is fine.
        apps.verify-results-csv = {
          type = "app";
          program = "${pkgs.writeShellScript "verify-results-csv" ''
            exec ${rEnv}/bin/Rscript ${./verify_results_csv.R} "$@"
          ''}";
        };
      });
}
