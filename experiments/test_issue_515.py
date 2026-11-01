"""Issue 515: two-value Link constructions mean (index, source), not (source, target)."""
from pathlib import Path
import shutil
import subprocess  # nosec B404 - Read fixed historical revisions from the local repository.
import sys
import unittest

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'scripts'))
import check_repository  # noqa: E402

CSHARP = 'csharp/Platform.Data.Doublets/ILinksExtensions.cs'
CPP = 'cpp/Platform.Data.Doublets/ILinksExtensions.h'


def lines(name, source):
    return [int(error.split(':')[1]) for error in check_repository.two_value_link_errors(name, source)]


class TwoValueLinkGuardTests(unittest.TestCase):
    def test_original_merge_usages_sites_are_rejected(self):
        # 77e2b6934 fixed C#; the C++ port kept the same defect until issue 515.
        for name, revision, expected in ((CSHARP, '77e2b6934^', [989, 1001]), (CPP, '2ecf56fe9', [854, 866])):
            with self.subTest(name=name):
                git = shutil.which('git') or 'git'
                source = subprocess.check_output([git, 'show', f'{revision}:{name}'], cwd=ROOT, text=True)  # nosec B603 B607 - Fixed local arguments.
                self.assertEqual(expected, lines(name, source))

    def test_csharp_constructions(self):
        self.assertEqual([1], lines(CSHARP, 'var s = new Link<TLinkAddress>(newLinkIndex, links.GetTarget(x));'))
        self.assertEqual([], lines(CSHARP, 'var s = new Link<TLinkAddress>(i, newLinkIndex, links.GetTarget(x));'))
        self.assertEqual([], lines(CSHARP, 'var s = new Link<T>(i); var e = new Link<T>();'))
        self.assertEqual([], lines(CSHARP, '// new Link<T>(a, b) in a comment'))

    def test_cpp_constructions_and_comma_operator(self):
        self.assertEqual([1], lines(CPP, 'typename TStorage::LinkType s = (newLinkIndex, GetTarget(storage, x));'))
        self.assertEqual([2], lines(CPP, '\nauto s = typename TStorage::LinkType(GetTarget(storage, x), n);'))
        self.assertEqual([1], lines(CPP, 'LinkType s{a, b};'))
        self.assertEqual([], lines(CPP, 'LinkType s{i, a, b}; LinkType r{i}; LinkType{};'))

    def test_tests_and_other_sources_may_build_two_value_queries(self):
        query = 'auto query = LinkType{any, value}; var q = new Link<T>(any, value);'
        self.assertEqual([], check_repository.two_value_link_errors('cpp/Platform.Data.Doublets.Tests/X.h', query))
        self.assertEqual([], check_repository.two_value_link_errors('csharp/Platform.Data.Doublets.Tests/X.cs', query))


if __name__ == '__main__':
    unittest.main()
